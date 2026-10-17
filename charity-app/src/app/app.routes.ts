import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard';
import { BeneficiariesComponent } from './beneficiaries/beneficiaries';
import { ItemsCatalogComponent } from './items-catalog/items-catalog';
import { DonationsComponent } from './donations/donations';
import { InventoryComponent } from './inventory/inventory';
import { DistributionsComponent } from './distributions/distributions';
import { DisbursementComponent } from './disbursement/disbursement';
import { SettingsComponent } from './settings/settings';
import { StaffComponent } from './staff/staff';
import { SuspensionReasonsComponent } from './suspension-reasons/suspension-reasons';
import { SeparationReasonsComponent } from './separation-reasons/separation-reasons';
import { BiAnalyticsComponent } from './bi-analytics/bi-analytics';
import { LoginComponent } from './login/login';
import { authGuard, adminGuard } from './auth.guard';
import { CharitiesAdminComponent } from './charities-admin/charities-admin';

export const routes: Routes = [
  { path: '', redirectTo: 'beneficiaries', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [adminGuard] },
  { path: 'bi-analytics', component: BiAnalyticsComponent, canActivate: [adminGuard] },
  { path: 'charities-admin', component: CharitiesAdminComponent, canActivate: [adminGuard] },
  { path: 'beneficiaries', component: BeneficiariesComponent, canActivate: [authGuard] },
  { path: 'suspension-reasons', component: SuspensionReasonsComponent, canActivate: [authGuard] },
  { path: 'separation-reasons', component: SeparationReasonsComponent, canActivate: [authGuard] },
  { path: 'items-catalog', component: ItemsCatalogComponent, canActivate: [authGuard] },
  { path: 'inventory', component: InventoryComponent, canActivate: [authGuard] },
  { path: 'donations', component: DonationsComponent, canActivate: [authGuard] },
  { path: 'disbursement', component: DisbursementComponent, canActivate: [authGuard] },
  { path: 'distributions', component: DistributionsComponent, canActivate: [authGuard] },
  { path: 'staff', component: StaffComponent, canActivate: [authGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] }
];
