import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html',
  styleUrls: ['./settings.css']
})
export class SettingsComponent implements OnInit {
  settings: any = { charityName: '', logo: '', details: '', phone: '' };
  saveMessage = '';

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.api.getSettings().subscribe((data: any) => { 
      if (data) {
        this.settings = data; 
      }
      this.cdr.detectChanges(); 
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.settings.logo = e.target.result;
        this.cdr.detectChanges();
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit() {
    this.api.updateSettings(this.settings).subscribe(() => {
      this.saveMessage = 'تم حفظ التعديلات بنجاح!';
      this.cdr.detectChanges();
      setTimeout(() => { this.saveMessage = ''; this.cdr.detectChanges(); }, 3000);
      
      // Request a hard refresh to update the global nav bar (in a real app we'd use a shared service/BehaviorSubject)
      setTimeout(() => { window.location.reload(); }, 1000);
    });
  }
}
