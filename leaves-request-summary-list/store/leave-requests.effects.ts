import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { map, switchMap, mergeMap, concatMap } from 'rxjs/operators';
import { Observable, of, forkJoin, throwError } from "rxjs";
import { HttpErrorResponse, HttpStatusCode } from "@angular/common/http";
import { getAll, getAllSuccess, getLeaveTypes, getLeaveTypesSuccess, getSummary, getSummarySuccess, getLeaveYearsSuccess, getLeaveYears } from "./leave-requests.actions";
import { LeaveRequestsService } from "../../../common/leave-requests.service";
import { ToastrService } from 'ngx-toastr';
// import { LeaveRequest } from "src/app/model/leave-request.model";

@Injectable({
    providedIn: 'root'
})
export class LeaveRequestEffects {
    constructor(private action$: Actions, private leaveRequestService: LeaveRequestsService, private toastService: ToastrService) { }

    protected handleError(error: HttpErrorResponse, continuation: () => Observable<any>) {
        if (error.status === HttpStatusCode.NotFound || error.status === HttpStatusCode.BadRequest) {
            return of(false);
        }
    };

    getAll$ = createEffect(() => this.action$.pipe(
        ofType(getAll),
        switchMap((payload) => {
            return this.leaveRequestService.getLeaveRequestByFilter(
                payload.userId, payload.pageNum,
                payload.pageSize, payload.filter).pipe(
                    map((responseData : any) => {
                        console.log('response in getAll Effect  ' + responseData.totalItems);

                        if (responseData) {
                            return getAllSuccess({ leaveRequests: responseData });
                        }
                        return getAllSuccess({ leaveRequests: {} });
                    })
            );
        }),
    ));

    getSummary$ = createEffect(() => this.action$.pipe(
        ofType(getSummary),
        switchMap((payload : any) => {
            console.log('payload in getSummary Effect: ' + JSON.stringify(payload));
            return this.leaveRequestService.getSummary(payload.year).pipe(
                map( (responseData : any[]) => {
                    if (responseData) {
                        return getSummarySuccess({ leaveSummaryItems: responseData });
                    }
                    return getSummarySuccess({ leaveSummaryItems: [] });
                })
            )
        }),
    ));

    getLeaveTypes$ = createEffect(() => this.action$.pipe(
        ofType(getLeaveTypes),
        switchMap((payload : any) => {
            return this.leaveRequestService.getLeaveTypes().pipe(
                map( (responseData : any[]) => {
                    if (responseData) {
                        return getLeaveTypesSuccess({ leaveTypes: responseData })
                    }
                    return getLeaveTypesSuccess({ leaveTypes: [] })
                })
            )
        }),
    ));

    getLeaveYears$ = createEffect(() => this.action$.pipe(
        ofType(getLeaveYears),
        switchMap((payload : any) => {
            return this.leaveRequestService.getLeaveYears().pipe(
                map( (responseData : any[]) => {
                    if (responseData) {
                        return getLeaveYearsSuccess({ leaveYears: responseData })
                    }
                    return getLeaveYearsSuccess({ leaveYears: [] })
                })
            )
        }),
    ));
}