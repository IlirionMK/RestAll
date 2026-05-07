import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:mobile/services/api_service.dart';

class AuthResult {
  final bool success;
  final bool requires2FA;
  final String? message;
  final String? role;

  AuthResult({required this.success, this.requires2FA = false, this.message, this.role});
}

class AuthService {
  String get baseUrl => ApiService.baseUrl;

  Future<AuthResult> login(String email, String password) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/auth/login'),
        headers: {'Accept': 'application/json', 'Content-Type': 'application/json'},
        body: jsonEncode({'email': email, 'password': password}),
      );

      final data = jsonDecode(response.body);

      if (response.statusCode == 200) {
        // RES-01: Obsługa 2FA
        if (data['two_factor'] == true) {
          return AuthResult(success: false, requires2FA: true);
        }

        // Zapisywanie tokenów
        ApiService.token = data['access_token'] ?? data['token'];
        ApiService.refreshToken = data['refresh_token'];
        
        // Zapisywanie danych użytkownika dla profilu (RES-21)
        ApiService.currentUser = data['user'];
        
        // RES-03: Weryfikacja praw dostępu (Role)
        String rawRole = 'guest';
        if (data['user'] != null && data['user']['role'] != null) {
          rawRole = data['user']['role'].toString().toLowerCase();
        } else if (data['role'] != null) {
          rawRole = data['role'].toString().toLowerCase();
        }
        ApiService.userRole = rawRole;
        
        return AuthResult(success: true, role: rawRole);
      } else {
        return AuthResult(success: false, message: data['message'] ?? "Błędny email lub hasło");
      }
    } catch (e) {
      return AuthResult(success: false, message: "Błąd połączenia: $e");
    }
  }

  Future<bool> register(String name, String email, String password) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/auth/register'),
        headers: {'Accept': 'application/json', 'Content-Type': 'application/json'},
        body: jsonEncode({
          'name': name, 
          'email': email, 
          'password': password, 
          'password_confirmation': password
        }),
      );
      
      if (response.statusCode == 201 || response.statusCode == 200) {
        // Automatyczne logowanie po rejestracji (opcjonalne, zależne od backendu)
        // Jeśli rejestracja nie loguje, użytkownik musi to zrobić ręcznie.
        return true;
      }
    } catch (e) {}
    return false;
  }

  Future<bool> resetPassword(String email) async {
    try {
      final response = await http.post(
        Uri.parse('${baseUrl.replaceAll('/api', '')}/forgot-password'),
        headers: {'Accept': 'application/json', 'Content-Type': 'application/json'},
        body: jsonEncode({'email': email}),
      );
      return response.statusCode == 200;
    } catch (e) {}
    return false;
  }

  Future<bool> verify2FA(String email, String code) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/auth/2fa/verify'),
        headers: {'Accept': 'application/json', 'Content-Type': 'application/json'},
        body: jsonEncode({'code': code}),
      );
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        ApiService.token = data['access_token'] ?? data['token'];
        return true;
      }
    } catch (e) {}
    return false;
  }
}
