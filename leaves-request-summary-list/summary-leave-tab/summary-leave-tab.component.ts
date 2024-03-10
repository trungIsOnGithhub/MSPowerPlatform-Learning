import { AfterViewInit, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';
import { LeaveRequestState } from '../store/leave-requests.state';
import { getLeaveTypes, getLeaveYears, getSummary } from '../store/leave-requests.actions';
import { takeUntil } from 'rxjs/operators';
import { getCurrentLeaveRequestsSummary, getCurrentLeaveTypes, getCurrentLeaveYears } from '../store/leave-requests.selectors';
import { User } from 'src/app/model/user.model';
import { Subject } from 'rxjs';
import { FormControl } from '@angular/forms';
import { LeaveRequestsService } from 'src/app/common/leave-requests.service';
import { getUserProfile } from 'src/app/shell/store';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-summary-leave-tab',
  templateUrl: './summary-leave-tab.component.html',
  styleUrls: ['./summary-leave-tab.component.scss']
})
export class SummaryLeaveTabComponent implements OnInit, OnDestroy, AfterViewInit {
  constructor(private store: Store<LeaveRequestState>) { }
  ngDestroyed$ = new Subject<void>();
  currentUser : User;
  displayedColumns : string[] = ['Avatar', 'Employee']

  isLoadingSummary : boolean = false;

  pageSizes = [2,3,4];

  // leaveSummaryByUser : any[] = [];
  // leaveTypeData : any[] = [];

  filteredSummaryData : any[] = [];
  dataSource: MatTableDataSource<any>;

  leaveTypeInfos : any[] = [];
  leaveYears : any[] = [];

  @ViewChild('paginator') paginator: MatPaginator;

  summaryYearFilterControl : FormControl = new FormControl(this.getCurrentLatestYear());

  ngOnInit(): void {
    this.store.select(getUserProfile)
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe((user) => {
        this.currentUser = user
      })

    this.store
      .select(getCurrentLeaveTypes)
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe(data => {
        this.leaveTypeInfos = this.getLeaveTypeInfos(data);
        this.displayedColumns = this.displayedColumns.concat([...this.leaveTypeInfos.map(typeInfo => typeInfo.typename)]);
      });

    this.store
      .select(getCurrentLeaveYears)
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe(data => {
        this.leaveYears = data;
      });

    this.summaryYearFilterControl.valueChanges
      .pipe(takeUntil(this.ngDestroyed$))
      .subscribe((year : number) => {
          if (!year) {
            this.store.dispatch(getSummary({ userId: this.currentUser.id, year: this.getCurrentLatestLeaveYear() }));
          } else {
            this.store.dispatch(getSummary({ userId: this.currentUser.id, year }));
          }
          this.paginator.pageIndex = 0;
      })

    this.store.dispatch(getLeaveYears());
  }

  ngAfterViewInit(): void {
    this.store
    .select(getCurrentLeaveRequestsSummary)
    .pipe(takeUntil(this.ngDestroyed$))
    .subscribe(data => {
      console.log('hearing getCurrentLeaveRequestsSummary: ' + JSON.stringify(data));
      this.filteredSummaryData = data;
      this.arrangeLeaveTrackDataByUser();

      this.dataSource = new MatTableDataSource(this.filteredSummaryData);
      this.dataSource.paginator = this.paginator;
    });

    this.store.dispatch(getSummary({
      userId: this.currentUser.id,
      year: this.getCurrentLatestYear()
    }));

    this.dataSource = new MatTableDataSource(this.filteredSummaryData);
    this.dataSource.paginator = this.paginator;
  }

  ngOnDestroy(): void {
    this.ngDestroyed$.next();
    this.ngDestroyed$.complete();
  }

  // Method to Prepare Data Format
  getLeaveTypeInfos(leaveTypeArray : any[]) : any[] {
    return leaveTypeArray.reduce((accumulator, current) => {
      accumulator.push({
        typeid: current.id,
        typename: current.leaveTypeLocalizations[0].name,
        baseNumberOfDay: current.baseNumberOfDay,
        maximumNumberOfDay: current.maximumNumberOfDay
      });
      return accumulator;
    }, []);
  }
  getAllUsersAvailableInLeaveTrack(leaveSummaryData : any[]) : any[] {
    return leaveSummaryData.reduce((accumulator, current) => {
      if (accumulator.filter(element => element.userid === current.userId).length > 0)
        return accumulator;

      accumulator.push({
        userid: current.userId,
        typename: current.user.name
      });
      return accumulator;
    }, []);
  }
  arrangeLeaveTrackDataByUser() : void {
    // let leaveTypesInfo = this.getLeaveTypeInfos(this.leaveTypeData);
    // console.log('in  arrangeLeaveTrackDataByUser: ' + JSON.stringify(leaveTypesInfo));
    let leaveTrackInfoByUserId = {};

    for (let track of this.filteredSummaryData) {
      if (!leaveTrackInfoByUserId[track.userId])
        leaveTrackInfoByUserId[track.userId] = {}

      for (let typeInfo of this.leaveTypeInfos) {
        if (typeInfo.typeid === track.leaveTypeId) {
          leaveTrackInfoByUserId[track.userId][typeInfo.typename] = {
            totalLeaveDay: track.totalLeaveDay,
            remainLeaveDay: track.remainLeaveDay
          }
          break;
        }
      }

      leaveTrackInfoByUserId[track.userId]['user'] = track['user'];
    }

    // console.log('inarrangeLeaveTrackDataByUser: '+JSON.stringify(leaveTrackInfoByUserId));

    this.filteredSummaryData = this.getAllUsersAvailableInLeaveTrack(this.filteredSummaryData)
    .reduce(
      (accumulator, current) => {
        accumulator.push(leaveTrackInfoByUserId[current.userid]);
        return accumulator;
      }, []
    );
  }
  // Method to Prepare Data Format


  resetFiltersValue() : void {
    this.summaryYearFilterControl.reset(this.getCurrentLatestLeaveYear());
  }


  getCurrentLatestLeaveYear() : number {
    return Math.max(...(this.leaveYears.map(record => record.year)));
  }
  getCurrentLatestYear() : number {
    return new Date().getFullYear()
  }
}