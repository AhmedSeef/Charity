import { Component, signal, ChangeDetectorRef, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Api } from './api';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App implements OnInit {
  mobileMenuOpen = false;
  title = signal('إدارة شبكة الجمعيات');
  logo = signal('');
  activeCharity = signal<any>(null);

  constructor(public api: Api, public auth: AuthService, private cdr: ChangeDetectorRef) {}

  toggleMobileMenu() { this.mobileMenuOpen = !this.mobileMenuOpen; }
  closeMobileMenu() { this.mobileMenuOpen = false; }

  ngOnInit() {
    this.initUserCharity();
  }

  initUserCharity() {
    const user = this.auth.getCurrentUser();
    const isAdmin = user?.role === 'Admin';
    if (user && user.charityId && user.role !== 'Admin') {
      this.api.setActiveCharityId(user.charityId);
    }

    this.api.getCharities().subscribe((list: any[]) => {
      const activeId = this.api.getActiveCharityId();
      const current = list.find(c => c.id === activeId) || list[0];
      this.activeCharity.set(current);
      if (current?.color) {
        document.documentElement.style.setProperty('--brand', current.color);
        document.documentElement.style.setProperty('--brand-600', `color-mix(in srgb, ${current.color} 82%, #000)`);
        document.documentElement.style.setProperty('--brand-50', `color-mix(in srgb, ${current.color} 10%, #fff)`);
      }
      if (current && !isAdmin) {
        this.title.set(current.name);
        this.logo.set(current.logo || '');
      }
      if (isAdmin) this.title.set('إدارة شبكة الجمعيات');
      this.cdr.detectChanges();
    });

    if (user?.role !== 'CharityManager') return;
    this.api.getSettings().subscribe((data: any) => {
      if (data && data.charityName) {
        this.title.set(data.charityName);
      }
      if (data && data.logo) {
        this.logo.set(data.logo);
      }
      this.cdr.detectChanges();
    });
  }

}
