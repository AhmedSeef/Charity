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
import { ExpenseCategoriesComponent } from './expense-categories/expense-categories';
import { BiAnalyticsComponent } from './bi-analytics/bi-analytics';
import { LoginComponent } from './login/login';
import { authGuard, adminGuard } from './auth.guard';
import { CharitiesAdminComponent } from './charities-admin/charities-admin';
import { AreaDistributionComponent } from './area-distribution/area-distribution';
import { VillagesManagementComponent } from './villages-management/villages-management';
import { AuditLogsComponent } from './audit-logs/audit-logs';
import { charityManagerGuard } from './auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'beneficiaries', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [adminGuard] },
  { path: 'bi-analytics', component: BiAnalyticsComponent, canActivate: [adminGuard] },
  { path: 'charities-admin', component: CharitiesAdminComponent, canActivate: [adminGuard] },
  { path: 'beneficiaries', component: BeneficiariesComponent, canActivate: [authGuard] },
  { path: 'villages-management', component: VillagesManagementComponent, canActivate: [authGuard] },
  { path: 'villages', component: VillagesManagementComponent, canActivate: [authGuard], data: { tab: 'villages' } },
  { path: 'villages/new', component: VillagesManagementComponent, canActivate: [authGuard], data: { formPage: true, tab: 'villages' } },
  { path: 'village-areas', component: VillagesManagementComponent, canActivate: [authGuard], data: { tab: 'areas' } },
  { path: 'village-areas/new', component: VillagesManagementComponent, canActivate: [authGuard], data: { formPage: true, tab: 'areas' } },
  { path: 'suspension-reasons', component: SuspensionReasonsComponent, canActivate: [authGuard] },
  { path: 'separation-reasons', component: SeparationReasonsComponent, canActivate: [authGuard] },
  { path: 'expense-categories', component: ExpenseCategoriesComponent, canActivate: [authGuard] },
  { path: 'expense-categories/new', component: ExpenseCategoriesComponent, canActivate: [authGuard], data: { formPage: true } },
  { path: 'items-catalog', component: ItemsCatalogComponent, canActivate: [authGuard] },
  { path: 'items-catalog/new', component: ItemsCatalogComponent, canActivate: [authGuard], data: { formPage: true } },
  { path: 'inventory', component: InventoryComponent, canActivate: [authGuard] },
  { path: 'donations', component: DonationsComponent, canActivate: [authGuard] },
  { path: 'donations/new', component: DonationsComponent, canActivate: [authGuard], data: { formPage: true } },
  { path: 'disbursement', component: DisbursementComponent, canActivate: [authGuard] },
  { path: 'area-distribution', component: AreaDistributionComponent, canActivate: [authGuard] },
  { path: 'distributions', component: DistributionsComponent, canActivate: [authGuard] },
  { path: 'staff', component: StaffComponent, canActivate: [authGuard] },
  { path: 'audit-logs', component: AuditLogsComponent, canActivate: [charityManagerGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] }
];
