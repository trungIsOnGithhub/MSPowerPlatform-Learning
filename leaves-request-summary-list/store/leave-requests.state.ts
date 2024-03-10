import { LeaveRequest } from '../../../model/leave-request.model';

export const initialState = {
    currentUserLeaveRequests: {},
    isLoading: true,

    currentLeaveRequestsSummary: [],
    isLoadingSummary: true,

    currentLeaveTypes: [],
    isLoadingLeaveTypes: true,

    currentLeaveYears: [],
    isLoadingLeaveYears: true,
}

export interface LeaveRequestState {
    currentUserLeaveRequests: any;
    isLoading: boolean;

    currentLeaveRequestsSummary: any[];
    isLoadingSummary: boolean;

    currentLeaveTypes: any[];
    isLoadingLeaveTypes: boolean;

    currentLeaveYears: any[];
    isLoadingLeaveYears: boolean;
}