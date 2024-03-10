import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LeavesRequestSummaryListComponent } from './leaves-request-summary-list.component';

describe('LeavesRequestSummaryListComponent', () => {
  let component: LeavesRequestSummaryListComponent;
  let fixture: ComponentFixture<LeavesRequestSummaryListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LeavesRequestSummaryListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LeavesRequestSummaryListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
