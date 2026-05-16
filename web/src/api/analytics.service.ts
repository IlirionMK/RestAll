import api from './axios';

export interface AnalyticsSummary {
    revenue: {
        today: number;
        this_week: number;
        this_month: number;
    };
    orders: {
        today: number;
        this_week: number;
        this_month: number;
        average_value: number;
    };
    top_items: {
        name: string;
        quantity_sold: number;
        revenue: number;
    }[];
    reservations: {
        today: number;
        this_week: number;
    };
}

export const AnalyticsService = {
    summary() {
        return api.get<AnalyticsSummary>('/api/analytics/summary');
    },
};
