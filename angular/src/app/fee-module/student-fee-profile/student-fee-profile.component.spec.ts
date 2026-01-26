import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentFeeProfileComponent } from './student-fee-profile.component';

describe('StudentFeeProfileComponent', () => {
  let component: StudentFeeProfileComponent;
  let fixture: ComponentFixture<StudentFeeProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StudentFeeProfileComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentFeeProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
