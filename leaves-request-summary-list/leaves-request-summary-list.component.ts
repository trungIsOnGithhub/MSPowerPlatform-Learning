import { AfterViewInit, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { LeaveRequest } from '../../model/leave-request.model';

import { LeaveRequestStatusEnum, LeaveRequestStatusViewValue } from 'src/app/common/enums/leave-request-status.enum';

import { LeaveRequestState } from './store/leave-requests.state';

import { BehaviorSubject, Subject } from 'rxjs';
import { User } from 'src/app/model/user.model';
import { FormControl } from '@angular/forms';
import { Store } from '@ngrx/store';
import { getIsLoadingState,  getCurrentUserLeaveRequest, getCurrentLeaveRequestsSummary, getCurrentLeaveTypes, getIsLoadingLeaveTypesState } from './store/leave-requests.selectors';
import { takeUntil } from 'rxjs/operators';

import { getAll, getLeaveTypes, getSummary } from './store/leave-requests.actions';
import { getUserProfile } from 'src/app/shell/store';
import { MatTableModule } from '@angular/material/table';

import { LoadingIconComponent } from '../../loading-icon/loading-icon.component'
import { MatPaginator } from '@angular/material/paginator';

// For further use
// import { LeaveRequestsService } from 'src/app/common/leave-requests.service';
// import { OrderDateByPipe } from './pipes/order-date-by.pipe';

export class LeaveRequestFilter {
  sortOrder: string = "";
  filterLeaveTypeId: number = -1;
  filterStatus: number = -1;
  filterStartDate: string = "";
  filterEndDate: string = "";
  orderBy: string = "";

  resetAllField() : void {
    this.sortOrder = "";
    this.filterLeaveTypeId = -1;
    this.filterStatus = -1;
    this.filterStartDate = "";
    this.filterEndDate = "";
    this.orderBy = "";  
  }
}

@Component({
  selector: 'app-leaves-request-summary-list',
  templateUrl: './leaves-request-summary-list.component.html',
  styleUrls: ['./leaves-request-summary-list.component.scss']
})
export class LeavesRequestSummaryListComponent implements OnInit, OnDestroy, AfterViewInit {
  constructor(private store: Store<LeaveRequestState>) { }
  pageSizes = [2,3,4];
  @ViewChild('paginator') paginator: MatPaginator;
  displayedColumns : string[] = ['Avatar', 'Requester', 'StartDate', 'EndDate', 'LeaveType', 'LeaveStatus', 'NumberOfDay', 'Approvers']

  sortProp = '';
  sortOrder = '';

  filteredData : any = [];

  currentFilter : LeaveRequestFilter = new LeaveRequestFilter();

  mainFilterArray : BehaviorSubject<any> = new BehaviorSubject([]);
  secondaryFilter : any = [];

  leaveTypes : any[] = [];

  approvalStatusValuesMap : any[] = [
    { value: String(LeaveRequestStatusEnum.Pending), viewValue: LeaveRequestStatusViewValue.Pending },
    { value: String(LeaveRequestStatusEnum.Approved), viewValue: LeaveRequestStatusViewValue.Approved },
    { value: String(LeaveRequestStatusEnum.Rejected), viewValue: LeaveRequestStatusViewValue.Rejected },
    { value: String(LeaveRequestStatusEnum.Waiting), viewValue: LeaveRequestStatusViewValue.Waiting },
    { value: String(LeaveRequestStatusEnum.Removed), viewValue: LeaveRequestStatusViewValue.Removed }
  ];

  isLoading: boolean = false;
  isLoadingLeaveTypes : boolean = false;

  currentUser : User;

  ngDestroyed$ = new Subject<void>();

  startDateFilterControl: FormControl = new FormControl();
  endDateFilterControl: FormControl = new FormControl();
  leaveTypeFilterControl : FormControl = new FormControl();
  leaveStatusFilterControl : FormControl = new FormControl();

  leaveTypeNameById : any = {};

  ngOnInit(): void {
    this.store.select(getUserProfile)
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe((user) => {
        this.currentUser = user
      })

    this.startDateFilterControl.valueChanges
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe((startDateValue : string) => {
        if (startDateValue)
          this.applyStartDateFilter(startDateValue);
      });

    this.endDateFilterControl.valueChanges
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe((endDateValue : string) => {
        if (endDateValue)
          this.applyEndDateFilter(endDateValue);
      });

    this.leaveTypeFilterControl.valueChanges
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe((leaveTypeValue : number) => {
        if (leaveTypeValue)
          this.applyLeaveTypeFilter(leaveTypeValue);
      });

    this.leaveStatusFilterControl.valueChanges
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe((leaveStatusValue : number) => {
        if (leaveStatusValue)
          this.applyLeaveStatusFilter(leaveStatusValue);
      });

    this.store
      .select(getIsLoadingState)
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe((data : boolean) => {
        this.isLoading = data;
      });
    this.store
      .select(getIsLoadingLeaveTypesState)
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe((data : boolean) => {
        this.isLoadingLeaveTypes = data;
      });

    this.store
      .select(getCurrentLeaveTypes)
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe((data : any[]) => {
        this.leaveTypes = data;
      });

    this.store.dispatch(getLeaveTypes());
    // this.store.dispatch(getAll({ userId: this.currentUser.id }));
  }

  ngAfterViewInit() {
    this.store
    .select(getCurrentUserLeaveRequest)
    .pipe(takeUntil(this.ngDestroyed$))
    .subscribe((data : any) => {
      // sorted by modfied time of request by default
      if (data && data.leaveRequests)
        this.filteredData = data.leaveRequests;

      this.paginator.length = data.totalItems;
    });

    this.paginator.page
    .pipe(takeUntil(this.ngDestroyed$))
    .subscribe((data : any) => {
      this.store.dispatch(getAll({ userId: this.currentUser.id, pageNum: this.paginator.pageIndex+1,
                                    pageSize: this.paginator.pageSize, filter: { ...this.currentFilter }  as LeaveRequestFilter }));
    });

    this.store.dispatch(getAll({ userId: this.currentUser.id, pageNum: this.paginator.pageIndex+1,
      pageSize: this.paginator.pageSize, filter: { ...this.currentFilter } as LeaveRequestFilter }));
  }

  ngOnDestroy(): void {
    this.ngDestroyed$.next();
    this.ngDestroyed$.complete();
  }


  // Filter code
  applyStartDateFilter(startDateValue : string) : void {
    this.currentFilter.filterStartDate = new Date(startDateValue).toLocaleDateString('en-GB');
    
    this.store.dispatch(getAll({ userId: this.currentUser.id, pageNum: 1,
      pageSize: this.paginator.pageSize, filter: { ...this.currentFilter } as LeaveRequestFilter }));
    this.paginator.pageIndex = 0;
  }
  applyEndDateFilter(endDateValue : string) : void {
    this.currentFilter.filterEndDate = new Date(endDateValue).toLocaleDateString('en-GB');
    // this.currentFilter = newFilterState;

    this.store.dispatch(getAll({ userId: this.currentUser.id, pageNum: 1,
      pageSize: this.paginator.pageSize, filter: { ...this.currentFilter } as LeaveRequestFilter }));
    this.paginator.pageIndex = 0;
  }
  applyLeaveTypeFilter(leaveTypeValue : number) : void {
    this.currentFilter.filterLeaveTypeId = leaveTypeValue;

    this.store.dispatch(getAll({ userId: this.currentUser.id, pageNum: 1,
      pageSize: this.paginator.pageSize, filter: { ...this.currentFilter } as LeaveRequestFilter }));
    this.paginator.pageIndex = 0;
  }
  applyLeaveStatusFilter(leaveStatusValue : number) : void {
    this.currentFilter.filterStatus = leaveStatusValue;

    this.store.dispatch(getAll({ userId: this.currentUser.id, pageNum: 1,
      pageSize: this.paginator.pageSize, filter: { ...this.currentFilter } as LeaveRequestFilter }));
    this.paginator.pageIndex = 0;
  }

  // Sort Function
  applySortPropAndOrder(orderBy : string, sortOrder : string) : void {
    this.currentFilter.orderBy = orderBy;
    this.currentFilter.sortOrder = sortOrder;

    this.store.dispatch(getAll({ userId: this.currentUser.id, pageNum: this.paginator.pageIndex+1,
      pageSize: this.paginator.pageSize, filter: { ...this.currentFilter } as LeaveRequestFilter }));
    this.paginator.pageIndex = 0;
  }
  // Sort Function


  // Reset Filter
  resetFilterValue() : void {
    this.startDateFilterControl.reset();
    this.endDateFilterControl.reset();

    this.leaveTypeFilterControl.reset();
    this.leaveStatusFilterControl.reset();

    this.currentFilter.resetAllField();

    this.store.dispatch(getAll({ userId: this.currentUser.id, pageNum: this.paginator.pageIndex+1,
      pageSize: this.paginator.pageSize, filter: { ...this.currentFilter } as LeaveRequestFilter }));

    this.mainFilterArray.next([]);
    // this.filteredData = this.queriedData;
  }
  // Reset Filter
}