import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventory.html',
  styleUrls: ['./inventory.css']
})
export class InventoryComponent implements OnInit {
  inventory: any[] = [];
  newItem: any = { itemName: '', quantity: 0, unit: 'كيلو', type: 'أغذية جافة' };
  units = ['كيلو', 'قطعة', 'علبة', 'زجاجة', 'كرتونة', 'شريط (دواء)'];
  types = ['أغذية جافة', 'طبي', 'لحوم ومجمدات', 'متنوع'];

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.api.getInventory().subscribe((data: any) => { this.inventory = data; this.cdr.detectChanges(); });
  }

  onSubmit() {
    if (!this.newItem.itemName) return;
    this.api.addInventory(this.newItem).subscribe(() => {
      this.loadData();
      this.newItem = { itemName: '', quantity: 0, unit: 'كيلو', type: 'أغذية جافة' };
    });
  }
}
