import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LateFeePolicyComponent } from './late-fee-policy.component';

describe('LateFeePolicyComponent', () => {
  let component: LateFeePolicyComponent;
  let fixture: ComponentFixture<LateFeePolicyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [LateFeePolicyComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LateFeePolicyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
