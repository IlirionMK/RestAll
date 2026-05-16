import api from './axios';

export interface LogEntry {
    id: number;
    user_id: number | null;
    action: string;
    model_type: string | null;
    model_id: number | null;
    payload: Record<string, any> | null;
    ip_address: string | null;
    created_at: string;
    user: { id: number; name: string; email: string } | null;
}

export interface LogsParams {
    action?: string;
    user_id?: number | string;
    date_from?: string;
    date_to?: string;
    per_page?: number;
    page?: number;
}

export const LogsService = {
    index(params?: LogsParams) {
        return api.get<{ data: LogEntry[]; current_page: number; last_page: number; total: number }>('/api/logs', { params });
    },
};
