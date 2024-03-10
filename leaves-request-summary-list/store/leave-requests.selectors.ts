import { createFeatureSelector, createSelector } from '@ngrx/store';
import * as stateName from '../../../common/constants/state-name.constants';
import { LeaveRequestState } from './leave-requests.state';
// import { LeaveRequest } from 'src/app/model/leave-request.model';

const getLeaveRequestState = createFeatureSelector<LeaveRequestState>(stateName.LEAVE_REQUEST_STATE);

export const getIsLoadingState = createSelector(getLeaveRequestState, (state: LeaveRequestState) => state.isLoading);
export const getCurrentUserLeaveRequest = createSelector(getLeaveRequestState, (state: LeaveRequestState) => state.currentUserLeaveRequests);

export const getIsLoadingStateSummary = createSelector(getLeaveRequestState, (state: LeaveRequestState) => state.isLoadingSummary);
export const getCurrentLeaveRequestsSummary = createSelector(getLeaveRequestState, (state: LeaveRequestState) => state.currentLeaveRequestsSummary);

export const getIsLoadingLeaveTypesState = createSelector(getLeaveRequestState, (state: LeaveRequestState) => state.isLoadingLeaveTypes);
export const getCurrentLeaveTypes = createSelector(getLeaveRequestState, (state: LeaveRequestState) => state.currentLeaveTypes);

export const getIsLoadingLeaveYearsState = createSelector(getLeaveRequestState, (state: LeaveRequestState) => state.isLoadingLeaveYears);
export const getCurrentLeaveYears = createSelector(getLeaveRequestState, (state: LeaveRequestState) => state.currentLeaveYears);