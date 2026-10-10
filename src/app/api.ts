import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Api {
  private baseUrl = 'http://localhost:3000';

  constructor(private http: HttpClient) {}

  getBeneficiaries(): Observable<any> { return this.http.get(`${this.baseUrl}/beneficiaries`); }
  addBeneficiary(data: any): Observable<any> { return this.http.post(`${this.baseUrl}/beneficiaries`, data); }
  updateBeneficiary(data: any): Observable<any> { return this.http.put(`${this.baseUrl}/beneficiaries/${data.id}`, data); }

  getDonations(): Observable<any> { return this.http.get(`${this.baseUrl}/donations`); }
  addDonation(data: any): Observable<any> { return this.http.post(`${this.baseUrl}/donations`, data); }

  getInventory(): Observable<any> { return this.http.get(`${this.baseUrl}/inventory`); }
  addInventory(data: any): Observable<any> { return this.http.post(`${this.baseUrl}/inventory`, data); }

  getDistributions(): Observable<any> { return this.http.get(`${this.baseUrl}/distributions`); }
  addDistribution(data: any): Observable<any> { return this.http.post(`${this.baseUrl}/distributions`, data); }

  getSettings(): Observable<any> { return this.http.get(`${this.baseUrl}/settings/1`); }
  updateSettings(data: any): Observable<any> { return this.http.put(`${this.baseUrl}/settings/1`, data); }

  getStaff(): Observable<any> { return this.http.get(`${this.baseUrl}/staff`); }
  addStaff(data: any): Observable<any> { return this.http.post(`${this.baseUrl}/staff`, data); }
  updateStaff(data: any): Observable<any> { return this.http.put(`${this.baseUrl}/staff/${data.id}`, data); }
}
