import { HomeComponent } from './home/home';
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
import { authGuard, adminGuard, permissionGuard, managerOrAdminGuard } from './auth.guard';
import { CharitiesAdminComponent } from './charities-admin/charities-admin';
import { AreaDistributionComponent } from './area-distribution/area-distribution';
import { VillagesManagementComponent } from './villages-management/villages-management';
import { AuditLogsComponent } from './audit-logs/audit-logs';
import { charityManagerGuard } from './auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent, canActivate: [authGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [adminGuard] },
  { path: 'bi-analytics', component: BiAnalyticsComponent, canActivate: [adminGuard] },
  { path: 'charities-admin', component: CharitiesAdminComponent, canActivate: [adminGuard] },
  { path: 'beneficiaries', component: BeneficiariesComponent, canActivate: [permissionGuard('beneficiaries.view')] },
  { path: 'villages-management', component: VillagesManagementComponent, canActivate: [permissionGuard('beneficiaries.create', 'beneficiaries.edit')] },
  { path: 'villages', component: VillagesManagementComponent, canActivate: [permissionGuard('beneficiaries.create', 'beneficiaries.edit')], data: { tab: 'villages' } },
  { path: 'villages/new', component: VillagesManagementComponent, canActivate: [permissionGuard('beneficiaries.create', 'beneficiaries.edit')], data: { formPage: true, tab: 'villages' } },
  { path: 'village-areas', component: VillagesManagementComponent, canActivate: [permissionGuard('beneficiaries.create', 'beneficiaries.edit')], data: { tab: 'areas' } },
  { path: 'village-areas/new', component: VillagesManagementComponent, canActivate: [permissionGuard('beneficiaries.create', 'beneficiaries.edit')], data: { formPage: true, tab: 'areas' } },
  { path: 'suspension-reasons', component: SuspensionReasonsComponent, canActivate: [managerOrAdminGuard] },
  { path: 'separation-reasons', component: SeparationReasonsComponent, canActivate: [managerOrAdminGuard] },
  { path: 'expense-categories', component: ExpenseCategoriesComponent, canActivate: [permissionGuard('expenses.manage')] },
  { path: 'expense-categories/new', component: ExpenseCategoriesComponent, canActivate: [permissionGuard('expenses.manage')], data: { formPage: true } },
  { path: 'items-catalog', component: ItemsCatalogComponent, canActivate: [permissionGuard('inventory.view', 'inventory.manage')] },
  { path: 'items-catalog/new', component: ItemsCatalogComponent, canActivate: [permissionGuard('inventory.manage')], data: { formPage: true } },
  { path: 'inventory', component: InventoryComponent, canActivate: [permissionGuard('inventory.view', 'inventory.manage')] },
  { path: 'donations', component: DonationsComponent, canActivate: [permissionGuard('donations.view')] },
  { path: 'donations/new', component: DonationsComponent, canActivate: [permissionGuard('donations.create')], data: { formPage: true } },
  { path: 'disbursement', component: DisbursementComponent, canActivate: [permissionGuard('disbursement.execute')] },
  { path: 'area-distribution', component: AreaDistributionComponent, canActivate: [permissionGuard('disbursement.execute')] },
  { path: 'distributions', component: DistributionsComponent, canActivate: [permissionGuard('disbursement.view')] },
  { path: 'staff', component: StaffComponent, canActivate: [permissionGuard('staff.manage')] },
  { path: 'audit-logs', component: AuditLogsComponent, canActivate: [charityManagerGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [charityManagerGuard] }
];
