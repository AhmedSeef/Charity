import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class Api {
  private apiUrl = (typeof window !== 'undefined' && window.location.port === '4200')
    ? 'http://localhost:5072/api'
    : '/api';
  private activeCharitySubject: BehaviorSubject<string>;
  public activeCharity$: Observable<string>;

  private cachedCharities: any[] = [];

  constructor(private http: HttpClient) {
    const savedActiveId = localStorage.getItem('active_charity_id') || 'c1';
    this.activeCharitySubject = new BehaviorSubject<string>(savedActiveId);
    this.activeCharity$ = this.activeCharitySubject.asObservable();
    this.fetchCharities();
  }

  // ==========================================
  // Charities & Active Tenant Management
  // ==========================================
  private fetchCharities(): void {
    this.http.get<any[]>(`${this.apiUrl}/charities`).pipe(
      catchError(() => of(this.cachedCharities))
    ).subscribe((list) => {
      if (list && list.length > 0) {
        this.cachedCharities = list;
      }
    });
  }

  getCharities(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/charities`).pipe(
      tap((list) => {
        if (list && list.length > 0) {
          this.cachedCharities = list;
        }
      }),
      catchError(() => of(this.cachedCharities))
    );
  }

  createCharity(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/charities`, data).pipe(
      tap(() => this.fetchCharities())
    );
  }

  createCharityManager(charityId: string, data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/charities/${charityId}/manager`, data);
  }

  getActiveCharityId(): string {
    return this.activeCharitySubject.value;
  }

  getActiveCharity(): any {
    const id = this.getActiveCharityId();
    const found = this.cachedCharities.find((c: any) => c.id === id);
    return found || this.cachedCharities[0] || { id, name: 'الجمعية', color: '#004d40', icon: '🏢' };
  }

  setActiveCharityId(id: string): void {
    const charity = this.cachedCharities.find((c: any) => c.id === id);
    if (charity || id) {
      localStorage.setItem('active_charity_id', id);
      this.activeCharitySubject.next(id);
    }
  }

  // ==========================================
  // 1. Beneficiaries (CRM & Family Records)
  // ==========================================
  getBeneficiaries(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/beneficiaries`);
  }

  getBeneficiaryById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/beneficiaries/${id}`);
  }

  getNextFamilyCode(): Observable<{ familyCode: string }> {
    return this.http.get<{ familyCode: string }>(`${this.apiUrl}/beneficiaries/next-family-code`);
  }

  addBeneficiary(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/beneficiaries`, data);
  }

  updateBeneficiary(data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/beneficiaries/${data.id}`, data);
  }

  suspendBeneficiary(beneficiaryId: string, reasonTitle: string, reasonId?: string, notes?: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/beneficiaries/${beneficiaryId}/suspend`, {
      reasonTitle: reasonTitle,
      suspensionReasonId: reasonId || null,
      notes: notes || ''
    });
  }

  reactivateBeneficiary(beneficiaryId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/beneficiaries/${beneficiaryId}/reactivate`, {});
  }

  addFamilyMember(beneficiaryId: string, member: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/beneficiaries/${beneficiaryId}/members`, member);
  }

  separateFamilyMember(beneficiaryId: string, memberId: string, createNewProfile: boolean = true, reason?: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/beneficiaries/${beneficiaryId}/members/separate`, {
      familyMemberId: memberId,
      reason: reason || 'استقلال بمسكن خاص وزواج',
      createNewBeneficiaryProfile: createNewProfile
    });
  }

  transferHeadship(beneficiaryId: string, dto: {
    newHeadFamilyMemberId: string;
    formerHeadAction?: 'KeepAsFamilyMember' | 'RemoveFromFamily';
    reason: string;
    formerHeadRelationship?: string;
    formerHeadSupportType?: string;
    newFamilyName?: string;
    notes?: string;
  }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/beneficiaries/${beneficiaryId}/transfer-headship`, dto);
  }

  deleteBeneficiary(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/beneficiaries/${id}`);
  }

  // ==========================================
  // 2. Suspension Reasons
  // ==========================================
  getSuspensionReasons(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/suspension-reasons`);
  }

  addSuspensionReason(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/suspension-reasons`, data);
  }

  deleteSuspensionReason(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/suspension-reasons/${id}`);
  }

  // ==========================================
  // 3. Aid Distributions
  // ==========================================
  getDistributions(onlyCurrentCharity: boolean = true): Observable<any[]> {
    const endpoint = onlyCurrentCharity ? 'my-charity' : 'all-network';
    return this.http.get<any[]>(`${this.apiUrl}/distributions/${endpoint}`);
  }

  getAllDistributions(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/distributions/all-network`);
  }

  getDistributionsForBeneficiary(benId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/distributions/beneficiary/${benId}`);
  }

  addDistribution(data: any): Observable<any> {
    const payload = {
      ...data,
      inventoryItemId: data.inventoryItemId || data.inventoryId || null
    };
    return this.http.post<any>(`${this.apiUrl}/distributions`, payload);
  }

  // ==========================================
  // 4. Inventory & Items Catalog
  // ==========================================
  getInventory(charityId?: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/inventory`);
  }

  getAllInventory(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/inventory`);
  }

  addInventory(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/inventory`, data);
  }

  updateInventory(data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/inventory/${data.id}`, data);
  }

  adjustStock(id: string, newQuantity: number, reason: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/inventory/${id}/adjust-stock`, { newQuantity, reason });
  }

  deleteInventory(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/inventory/${id}`);
  }

  // ==========================================
  // 5. Donations
  // ==========================================
  getDonations(charityId?: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/donations`);
  }

  addDonation(data: any): Observable<any> {
    const payload = {
      ...data,
      inventoryItemId: data.inventoryItemId || data.inventoryId || null
    };
    return this.http.post<any>(`${this.apiUrl}/donations`, payload);
  }

  // ==========================================
  // 6. Staff & Volunteers
  // ==========================================
  getStaff(charityId?: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/staff`);
  }

  addStaff(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/staff`, data);
  }

  updateStaff(data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/staff/${data.id}`, data);
  }

  deleteStaff(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/staff/${id}`);
  }

  // ==========================================
  // 7. Settings
  // ==========================================
  getSettings(charityId?: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/charities/my-settings`);
  }

  updateSettings(data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/charities/my-settings`, data);
  }

  // ==========================================
  // 8. BI & Executive Analytics
  // ==========================================
  getBiAnalytics(charityId?: string): Observable<any> {
    const query = charityId ? `?charityId=${charityId}` : '';
    return this.http.get<any>(`${this.apiUrl}/analytics/bi${query}`);
  }

  getDashboardStats(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/dashboard/stats`);
  }
}
