import { createAction, props } from '@ngrx/store';
import { LeaveRequest } from '../../../model/leave-request.model';
import { LeaveRequestFilter } from '../leaves-request-summary-list.component';

const PREFIX = '[LEAVE REQUESTS LISTINGS]';
export const getAll = createAction(`${PREFIX} Get all current user leave request`, props<{ userId : number, pageNum : number, pageSize : number, filter : LeaveRequestFilter }>());
export const getAllSuccess = createAction(`${PREFIX} Get all current user leave request successfully`, props<{ leaveRequests: any }>());

export const getSummary = createAction(`${PREFIX} Get user leave summary by permission`, props<{ userId : number, year : number }>());
export const getSummarySuccess = createAction(`${PREFIX} Get user leave summary by permission successfully`, props<{ leaveSummaryItems : any[] }>());

export const getLeaveTypes = createAction(`${PREFIX} Get leave types and description`);
export const getLeaveTypesSuccess = createAction(`${PREFIX} Get leave types and description successfully`, props<{ leaveTypes : any[] }>());

export const getLeaveYears = createAction(`${PREFIX} Get all current user leave years`);
export const getLeaveYearsSuccess = createAction(`${PREFIX} Get all current user leave years successfully`, props<{ leaveYears: any[] }>());