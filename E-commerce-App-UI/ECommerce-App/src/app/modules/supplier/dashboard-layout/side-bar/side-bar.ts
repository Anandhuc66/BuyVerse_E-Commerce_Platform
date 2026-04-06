import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-side-bar',
  standalone: false,
  templateUrl: './side-bar.html',
  styleUrl: './side-bar.css'
})
export class SideBar {
@Output() linkClicked = new EventEmitter<void>();
  @Input() isCollapsed = false;
  
  // Track which dropdowns are open
  openDropdown: string | null = null;
  
  openDropdowns: Set<string> = new Set();

  toggleDropdown(menu: string) {
    // toggle open/close
    // this.openDropdown = this.openDropdown === menu ? null : menu;
    if (this.openDropdowns.has(menu)) {
      this.openDropdowns.delete(menu); // close it if open
    } else {
      this.openDropdowns.add(menu); // open it
    }
  }
  
  isDropdownOpen(menu: string): boolean {
    return this.openDropdowns.has(menu);
  }

  onLinkClick() {
    this.linkClicked.emit();
  }
}
