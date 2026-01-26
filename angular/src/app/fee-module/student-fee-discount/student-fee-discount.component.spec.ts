import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentFeeDiscountComponent } from './student-fee-discount.component';

describe('StudentFeeDiscountComponent', () => {
  let component: StudentFeeDiscountComponent;
  let fixture: ComponentFixture<StudentFeeDiscountComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StudentFeeDiscountComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentFeeDiscountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
