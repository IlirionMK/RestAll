import 'dart:convert';
import 'package:http/http.dart' as http;
import 'dart:io';

class ApiService {
  static String get baseUrl {
    if (Platform.isAndroid) return 'http://10.0.2.2:8000/api';
    return 'http://localhost:8000/api';
  }

  static String? token;
  static String? refreshToken;
  static String? userRole;
  static Map<String, dynamic>? currentUser;

  Map<String, String> _getHeaders() => {
    'Authorization': 'Bearer $token',
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  };

  // RES-02: Odświeżanie tokena
  Future<bool> attemptTokenRefresh() async {
    if (refreshToken == null) return false;
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/auth/refresh'),
        headers: {'Accept': 'application/json'},
        body: jsonEncode({'refresh_token': refreshToken}),
      );
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        token = data['access_token'] ?? data['token'];
        return true;
      }
    } catch (e) {
      return false;
    }
    return false;
  }

  // RES-04: Pobieranie menu i kategorii
  Future<List<dynamic>> getCategories() async {
    try {
      final response = await http.get(Uri.parse('$baseUrl/menu/categories'), headers: _getHeaders());
      if (response.statusCode == 200) return jsonDecode(response.body);
    } catch (e) {}
    return [];
  }

  Future<List<dynamic>> getMenuItems({int? categoryId}) async {
    try {
      String url = '$baseUrl/menu/items';
      if (categoryId != null) url += '?category_id=$categoryId';
      final response = await http.get(Uri.parse(url), headers: _getHeaders());
      if (response.statusCode == 200) return jsonDecode(response.body);
    } catch (e) {}
    return [];
  }

  // RES-07, RES-08: Stoliki i ich statusy
  Future<List<dynamic>> getTables() async {
    try {
      final response = await http.get(Uri.parse('$baseUrl/tables'), headers: _getHeaders());
      if (response.statusCode == 200) return jsonDecode(response.body);
    } catch (e) {}
    return [];
  }

  Future<String?> updateTableStatus(int tableId, String status) async {
    try {
      final response = await http.patch(
        Uri.parse('$baseUrl/tables/$tableId/status'),
        headers: _getHeaders(),
        body: jsonEncode({'status': status.toLowerCase()}),
      );
      if (response.statusCode == 200 || response.statusCode == 204) return null;
      return "Błąd: ${response.statusCode}";
    } catch (e) {
      return "Błąd połączenia";
    }
  }

  // RES-09: Rezerwacje
  Future<List<dynamic>> getReservations() async {
    try {
      final response = await http.get(Uri.parse('$baseUrl/reservations'), headers: _getHeaders());
      if (response.statusCode == 200) return jsonDecode(response.body);
    } catch (e) {}
    return [];
  }

  Future<bool> createReservation(int tableId, String date) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/reservations'),
        headers: _getHeaders(),
        body: jsonEncode({
          'table_id': tableId,
          'reservation_date': date, // Format ISO8601
        }),
      );
      return response.statusCode == 201 || response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<bool> deleteReservation(int id) async {
    try {
      final response = await http.delete(Uri.parse('$baseUrl/reservations/$id'), headers: _getHeaders());
      return response.statusCode == 204 || response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  // RES-11, RES-12, RES-17: Zamówienia
  Future<bool> createOrder(int tableId, List<Map<String, dynamic>> items) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/orders'),
        headers: _getHeaders(),
        body: jsonEncode({
          'table_id': tableId,
          'items': items.map((i) => {
            'menu_item_id': i['id'],
            'quantity': i['quantity'] ?? 1,
            'comment': i['comment'] ?? ""
          }).toList()
        }),
      );
      return response.statusCode == 201 || response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  // RES-13: Rachunek
  Future<bool> payOrder(int orderId) async {
    try {
      final response = await http.patch(Uri.parse('$baseUrl/orders/$orderId/pay'), headers: _getHeaders());
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  // RES-21: Edycja profilu
  Future<bool> updateProfile(String name, String email) async {
    try {
      final response = await http.put(
        Uri.parse('$baseUrl/users/me'),
        headers: _getHeaders(),
        body: jsonEncode({'name': name, 'email': email}),
      );
      if (response.statusCode == 200) {
        // Po udanej aktualizacji na serwerze, odświeżamy dane lokalne
        currentUser?['name'] = name;
        currentUser?['email'] = email;
        return true;
      }
    } catch (e) {}
    return false;
  }

  // Zmiana hasła (Fortify standard)
  Future<bool> changePassword(String currentPassword, String newPassword) async {
    try {
      final response = await http.put(
        Uri.parse('${baseUrl.replaceAll('/api', '')}/user/password'),
        headers: _getHeaders(),
        body: jsonEncode({
          'current_password': currentPassword,
          'password': newPassword,
          'password_confirmation': newPassword,
        }),
      );
      return response.statusCode == 200;
    } catch (e) {}
    return false;
  }

  // RES-16: Powiadomienia push (Symulacja / Metoda do podpięcia)
  void initPushNotifications() {
    print("Inicjalizacja powiadomień Push (RES-16)");
    // Tu zazwyczaj podpinamy Firebase Messaging
  }
}
