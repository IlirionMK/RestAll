import api from './axios';

export interface StaffMember {
    id: number;
    name: string;
    email: string;
    role: string;
    restaurant_id: number | null;
}

export interface StoreStaffData {
    name: string;
    email: string;
    password: string;
    role: string;
}

export const StaffService = {
    index() {
        return api.get<StaffMember[]>('/api/users');
    },

    store(data: StoreStaffData) {
        return api.post<StaffMember>('/api/users', data);
    },

    updateRole(userId: number | string, role: string) {
        return api.patch<StaffMember>(`/api/users/${userId}/role`, { role });
    },

    destroy(userId: number | string) {
        return api.delete(`/api/users/${userId}`);
    },
};
