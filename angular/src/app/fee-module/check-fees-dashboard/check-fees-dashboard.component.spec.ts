import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckFeesDashboardComponent } from './check-fees-dashboard.component';

describe('CheckFeesDashboardComponent', () => {
  let component: CheckFeesDashboardComponent;
  let fixture: ComponentFixture<CheckFeesDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CheckFeesDashboardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CheckFeesDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
