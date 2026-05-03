import { AuthService } from './auth.service';
import { KitchenService } from './kitchen.service';
import { OrdersService } from './orders.service';
import { ReservationsService } from './reservations.service';
import { TablesService } from './tables.service';

export const API = {
    auth: AuthService,
    kitchen: KitchenService,
    orders: OrdersService,
    reservations: ReservationsService,
    tables: TablesService
};