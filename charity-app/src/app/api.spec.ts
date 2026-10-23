import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Api } from './api';
import { vi } from 'vitest';

describe('Api request contracts', () => {
  let api: Api;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    api = TestBed.inject(Api);
    http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/charities').flush([]);
  });

  afterEach(() => http.verify());

  it('maps the UI inventory selection to the backend donation contract', () => {
    api.addDonation({ donorName: 'متبرع', donationKind: 'عيني', inventoryId: 'item-1', quantity: 3 }).subscribe();
    const request = http.expectOne('/api/donations');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.inventoryItemId).toBe('item-1');
    request.flush({});
  });

  it('loads cross-charity history through the selected beneficiary profile', () => {
    api.getBeneficiarySharedHistory('ahmed').subscribe();
    http.expectOne('/api/beneficiaries/ahmed/aid-history').flush({ aidHistory: [] });
  });

  it('maps the UI inventory selection to the backend distribution contract', () => {
    api.addDistribution({ beneficiaryId: 'family-1', inventoryId: 'item-1', quantity: 1 }).subscribe();
    const request = http.expectOne('/api/distributions');
    expect(request.request.body.inventoryItemId).toBe('item-1');
    request.flush({});
  });

  it('sends stock adjustments through the dedicated endpoint', () => {
    api.adjustStock('item-1', 7, 'جرد').subscribe();
    const request = http.expectOne('/api/inventory/item-1/adjust-stock');
    expect(request.request.body).toEqual({ newQuantity: 7, reason: 'جرد' });
    request.flush(null);
  });

  for (const bulk of [false, true]) {
    it(`confirms an overdraft and retries the same ${bulk ? 'bulk' : 'single'} request once`, () => {
      const confirm = vi.spyOn(window, 'confirm').mockReturnValue(true);
      const data = bulk ? { beneficiaryIds: ['family-1'], inventoryItemId: 'item-1', quantityPerBeneficiary: 3 }
        : { beneficiaryId: 'family-1', inventoryItemId: 'item-1', quantity: 3 };
      const url = '/api/distributions' + (bulk ? '/bulk' : '');
      let result: any;
      (bulk ? api.createBulkDistribution(data) : api.addDistribution(data)).subscribe(value => result = value);
      http.expectOne(url).flush({ code: 'balance_confirmation_required', message: 'سيصبح الرصيد -2' }, { status: 409, statusText: 'Conflict' });
      const retry = http.expectOne(url);
      expect(retry.request.body).toEqual({ ...data, allowNegativeBalance: true });
      retry.flush({ remainingStock: -2 });
      expect(result.remainingStock).toBe(-2);
      expect(confirm).toHaveBeenCalledOnce();
      confirm.mockRestore();
    });

    it(`cancels a ${bulk ? 'bulk' : 'single'} overdraft without retrying`, () => {
      const confirm = vi.spyOn(window, 'confirm').mockReturnValue(false);
      const url = '/api/distributions' + (bulk ? '/bulk' : '');
      let error: any;
      (bulk ? api.createBulkDistribution({ beneficiaryIds: ['family-1'] }) : api.addDistribution({ beneficiaryId: 'family-1' }))
        .subscribe({ error: err => error = err });
      http.expectOne(url).flush({ code: 'balance_confirmation_required', message: 'عجز' }, { status: 409, statusText: 'Conflict' });
      http.expectNone(url);
      expect(error.status).toBe(409);
      confirm.mockRestore();
    });
  }
});
