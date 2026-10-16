import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Api } from './api';

describe('Api request contracts', () => {
  let api: Api;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    api = TestBed.inject(Api);
    http = TestBed.inject(HttpTestingController);
    http.expectOne('http://localhost:5072/api/charities').flush([]);
  });

  afterEach(() => http.verify());

  it('maps the UI inventory selection to the backend donation contract', () => {
    api.addDonation({ donorName: 'متبرع', donationKind: 'عيني', inventoryId: 'item-1', quantity: 3 }).subscribe();
    const request = http.expectOne('http://localhost:5072/api/donations');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.inventoryItemId).toBe('item-1');
    request.flush({});
  });

  it('maps the UI inventory selection to the backend distribution contract', () => {
    api.addDistribution({ beneficiaryId: 'family-1', inventoryId: 'item-1', quantity: 1 }).subscribe();
    const request = http.expectOne('http://localhost:5072/api/distributions');
    expect(request.request.body.inventoryItemId).toBe('item-1');
    request.flush({});
  });

  it('sends stock adjustments through the dedicated endpoint', () => {
    api.adjustStock('item-1', 7, 'جرد').subscribe();
    const request = http.expectOne('http://localhost:5072/api/inventory/item-1/adjust-stock');
    expect(request.request.body).toEqual({ newQuantity: 7, reason: 'جرد' });
    request.flush(null);
  });
});
