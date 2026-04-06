import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupplierAdd } from './supplier-add';

describe('SupplierAdd', () => {
  let component: SupplierAdd;
  let fixture: ComponentFixture<SupplierAdd>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SupplierAdd]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupplierAdd);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
