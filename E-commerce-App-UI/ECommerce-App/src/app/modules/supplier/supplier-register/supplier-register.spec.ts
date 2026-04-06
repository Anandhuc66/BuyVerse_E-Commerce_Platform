import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupplierRegister } from './supplier-register';

describe('SupplierRegister', () => {
  let component: SupplierRegister;
  let fixture: ComponentFixture<SupplierRegister>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SupplierRegister]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupplierRegister);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
