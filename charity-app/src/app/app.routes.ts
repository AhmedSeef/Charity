import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard';
import { BeneficiariesComponent } from './beneficiaries/beneficiaries';
import { ItemsCatalogComponent } from './items-catalog/items-catalog';
import { DonationsComponent } from './donations/donations';
import { InventoryComponent } from './inventory/inventory';
import { DistributionsComponent } from './distributions/distributions';
import { SettingsComponent } from './settings/settings';
import { StaffComponent } from './staff/staff';
import { SuspensionReasonsComponent } from './suspension-reasons/suspension-reasons';
import { BiAnalyticsComponent } from './bi-analytics/bi-analytics';
import { LoginComponent } from './login/login';
import { authGuard } from './auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'bi-analytics', component: BiAnalyticsComponent, canActivate: [authGuard] },
  { path: 'beneficiaries', component: BeneficiariesComponent, canActivate: [authGuard] },
  { path: 'suspension-reasons', component: SuspensionReasonsComponent, canActivate: [authGuard] },
  { path: 'items-catalog', component: ItemsCatalogComponent, canActivate: [authGuard] },
  { path: 'inventory', component: InventoryComponent, canActivate: [authGuard] },
  { path: 'donations', component: DonationsComponent, canActivate: [authGuard] },
  { path: 'distributions', component: DistributionsComponent, canActivate: [authGuard] },
  { path: 'staff', component: StaffComponent, canActivate: [authGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] }
];
