import api from './axios';

export interface OrderItemInput {
    menu_item_id: number;
    quantity: number;
    price: number | string;
}

export interface OrderCreateData {
    table_id: number;
    reservation_id?: number | null;
    items?: OrderItemInput[];
}

export interface OrderContext {
    has_active_reservation: boolean;
    context: {
        reservation_id: number;
        table_id: number;
        table_number: string | number;
        restaurant_id: number;
        restaurant_name: string;
    } | null;
}

export const OrdersService = {
    getContext() {
        return api.get<OrderContext>('/api/orders/context');
    },

    index() {
        return api.get('/api/orders');
    },

    store(data: OrderCreateData) {
        return api.post('/api/orders', data);
    },

    show(id: number | string) {
        return api.get(`/api/orders/${id}`);
    },

    addItems(id: number | string, items: OrderItemInput[]) {
        return api.post(`/api/orders/${id}/items`, { items });
    },

    removeItem(orderItemId: number) {
        return api.delete(`/api/orders/items/${orderItemId}`);
    },

    pay(id: number | string) {
        return api.patch(`/api/orders/${id}/pay`);
    },

    requestBill(id: number | string) {
        return api.patch(`/api/orders/${id}/request-bill`);
    }
};