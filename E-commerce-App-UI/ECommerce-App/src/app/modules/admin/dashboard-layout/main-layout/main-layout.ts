import { Component } from '@angular/core';

@Component({
  selector: 'app-main-layout',
  standalone: false,
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css'
})
export class MainLayout {
  isCollapsed = false;

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
  }
isMobileOpen = false;     // mobile slide-in

onToggleSidebar() {
  // if mobile viewport, slide in/out; else collapse width
  if (window.innerWidth < 992) {
    this.isMobileOpen = !this.isMobileOpen;
  } else {
    this.isCollapsed = !this.isCollapsed;
  }
}

onSidebarLinkClicked() {
  // close the drawer on mobile after navigation
  if (window.innerWidth < 992) this.isMobileOpen = false;
}


}
