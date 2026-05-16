import api from './axios';

export const UserService = {
    updateMe(data: { name: string }) {
        return api.put('/api/users/me', data);
    },
};
