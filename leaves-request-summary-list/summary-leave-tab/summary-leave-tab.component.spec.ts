import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SummaryLeaveTabComponent } from './summary-leave-tab.component';

describe('SummaryLeaveTabComponent', () => {
  let component: SummaryLeaveTabComponent;
  let fixture: ComponentFixture<SummaryLeaveTabComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SummaryLeaveTabComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SummaryLeaveTabComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
