import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CatNavbar } from './cat-navbar';

describe('CatNavbar', () => {
  let component: CatNavbar;
  let fixture: ComponentFixture<CatNavbar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CatNavbar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CatNavbar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
