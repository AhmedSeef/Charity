import { Component, signal, ChangeDetectorRef, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from './api';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App implements OnInit {
  title = signal('جمعية أبواب الخير');
  logo = signal('');
  charities = signal<any[]>([]);
  activeCharityId = signal<string>('c1');
  activeCharity = signal<any>(null);

  constructor(public api: Api, public auth: AuthService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadCharityInfo();
  }

  loadCharityInfo() {
    this.api.getCharities().subscribe((list: any[]) => {
      this.charities.set(list);
      const activeId = this.api.getActiveCharityId();
      this.activeCharityId.set(activeId);
      const current = list.find(c => c.id === activeId) || list[0];
      this.activeCharity.set(current);
      if (current) {
        this.title.set(current.name);
        this.logo.set(current.logo || '');
      }
      this.cdr.detectChanges();
    });

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

  onCharityChange(newId: string) {
    if (!newId || newId === this.activeCharityId()) return;
    this.api.setActiveCharityId(newId);
    this.activeCharityId.set(newId);
    
    // Update currentUser charity if logged in
    const user = this.auth.getCurrentUser();
    if (user) {
      user.charityId = newId;
      const ch = this.charities().find(c => c.id === newId);
      if (ch) user.charityName = ch.name;
      localStorage.setItem('currentUser', JSON.stringify(user));
    }

    // Quick reload to cleanly refresh views with the chosen charity's isolated records
    window.location.reload();
  }
}
