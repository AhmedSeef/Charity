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
  title = signal('جمعية أبواب الخير');
  logo = signal('');

  constructor(private api: Api, public auth: AuthService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
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
