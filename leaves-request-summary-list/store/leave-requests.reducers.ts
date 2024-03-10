import { createReducer, on } from "@ngrx/store";
import { getAll, getAllSuccess, getLeaveTypes, getLeaveTypesSuccess, getSummary, getSummarySuccess, getLeaveYears, getLeaveYearsSuccess } from "./leave-requests.actions";
import { initialState, LeaveRequestState } from "./leave-requests.state";

export const LeaveRequestReducer = createReducer(
    initialState,

    on(getAll, (state, payload) => {
        return {
            ...state,
            isLoading: true,
            currentUserLeaveRequests: {}
        }
    }),

    on(getAllSuccess, (state, payload) => {
        return {
            ...state,
            isLoading: false,
            currentUserLeaveRequests: { ...payload.leaveRequests }
        }
    }),

    on(getSummary, (state, payload) => {
        return {
            ...state,
            isLoadingSummary: true,
            currentLeaveRequestsSummary: []
        }
    }),

    on(getSummarySuccess, (state, payload) => {
        return {
            ...state,
            isLoadingSummary: false,
            currentLeaveRequestsSummary: [...payload.leaveSummaryItems]
        }
    }),

    on(getLeaveTypes, (state, payload) => {
        return {
            ...state,
            isLoadingLeaveTypes: true,
            currentLeaveRequestsSummary: []
        }
    }),

    on(getLeaveTypesSuccess, (state, payload) => {
        return {
            ...state,
            isLoadingLeaveTypes: false,
            currentLeaveTypes: [...payload.leaveTypes]
        }
    }),

    on(getLeaveYears, (state, payload) => {
        return {
            ...state,
            isLoadingLeaveYears: true,
            currentLeaveYears: []
        }
    }),

    on(getLeaveYearsSuccess, (state, payload) => {
        return {
            ...state,
            isLoadingLeaveYears: false,
            currentLeaveYears: [...payload.leaveYears]
        }
    }), 
);