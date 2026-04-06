import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Homepage1 } from './homepage1';

describe('Homepage1', () => {
  let component: Homepage1;
  let fixture: ComponentFixture<Homepage1>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Homepage1]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Homepage1);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
