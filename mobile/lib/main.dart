import 'package:flutter/material.dart';
import 'screens/waiter/tables_screen.dart';
import 'screens/waiter/register_screen.dart';
import 'services/auth_service.dart';
import 'services/api_service.dart';

void main() {
  runApp(const RestAllApp());
}

class RestAllApp extends StatelessWidget {
  const RestAllApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'RestAll',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: Colors.deepOrange,
          primary: Colors.deepOrange,
          secondary: Colors.orangeAccent,
        ),
        inputDecorationTheme: InputDecorationTheme(
          filled: true,
          fillColor: Colors.grey[100],
          border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide.none),
          enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide.none),
          focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: Colors.deepOrange, width: 2)),
          labelStyle: const TextStyle(color: Colors.blueGrey),
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            backgroundColor: Colors.deepOrange,
            foregroundColor: Colors.white,
            minimumSize: const Size.fromHeight(55),
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            textStyle: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
          ),
        ),
      ),
      home: const LoginScreen(),
    );
  }
}

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});
  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _email = TextEditingController();
  final _pass = TextEditingController();
  bool _isLoading = false;
  bool _obscure = true;

  void _login() async {
    if (_email.text.isEmpty || _pass.text.isEmpty) return;
    setState(() => _isLoading = true);
    final result = await AuthService().login(_email.text.trim(), _pass.text.trim());
    setState(() => _isLoading = false);

    if (result.requires2FA) {
      _show2FADialog(_email.text.trim());
    } else if (result.success) {
      if (mounted) Navigator.pushReplacement(context, MaterialPageRoute(builder: (_) => const TablesScreen()));
    } else {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(result.message ?? "Błędne dane logowania"), backgroundColor: Colors.redAccent),
        );
      }
    }
  }

  void _show2FADialog(String email) {
    final code = TextEditingController();
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        title: const Text("Weryfikacja dwuetapowa (2FA)"),
        content: TextField(
          controller: code,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(labelText: "Wprowadź 6-cyfrowy kod", hintText: "123456"),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text("Anuluj")),
          ElevatedButton(
            onPressed: () async {
              bool ok = await AuthService().verify2FA(email, code.text);
              if (ok && mounted) {
                Navigator.pop(context);
                Navigator.pushReplacement(context, MaterialPageRoute(builder: (_) => const TablesScreen()));
              }
            },
            child: const Text("Weryfikuj"),
          )
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 30, vertical: 50),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Center(child: Icon(Icons.restaurant_menu, size: 100, color: Colors.deepOrange)),
              const SizedBox(height: 40),
              const Text("Witaj ponownie!", style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold)),
              const Text("Zaloguj się do panelu zarządzania restauracją", style: TextStyle(color: Colors.grey)),
              const SizedBox(height: 40),
              TextField(
                controller: _email,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(labelText: 'Adres Email', prefixIcon: Icon(Icons.email_outlined)),
              ),
              const SizedBox(height: 20),
              TextField(
                controller: _pass,
                obscureText: _obscure,
                decoration: InputDecoration(
                  labelText: 'Hasło',
                  prefixIcon: const Icon(Icons.lock_outline),
                  suffixIcon: IconButton(icon: Icon(_obscure ? Icons.visibility_off : Icons.visibility), onPressed: () => setState(() => _obscure = !_obscure)),
                ),
              ),
              const SizedBox(height: 10),
              Align(
                alignment: Alignment.centerRight,
                child: TextButton(
                  onPressed: () async {
                    if (_email.text.isEmpty) {
                      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Najpierw wprowadź swój email")));
                      return;
                    }
                    bool ok = await AuthService().resetPassword(_email.text);
                    if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(ok ? "Link do resetu wysłany na email" : "Błąd podczas wysyłania linku")));
                  },
                  child: const Text("Zapomniałeś hasła?"),
                ),
              ),
              const SizedBox(height: 20),
              _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : ElevatedButton(onPressed: _login, child: const Text("ZALOGUJ SIĘ")),
              const SizedBox(height: 20),
              const Center(child: Text("LUB", style: TextStyle(color: Colors.grey))),
              const SizedBox(height: 20),
              OutlinedButton.icon(
                style: OutlinedButton.styleFrom(minimumSize: const Size.fromHeight(55), shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))),
                icon: const Icon(Icons.g_mobiledata, size: 30),
                label: const Text("Kontynuuj przez Google"),
                onPressed: () {
                   ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Logowanie Google wkrótce...")));
                },
              ),
              const SizedBox(height: 30),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Text("Nie masz konta? "),
                  GestureDetector(
                    onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const RegisterScreen())),
                    child: const Text("Zarejestruj się", style: TextStyle(color: Colors.deepOrange, fontWeight: FontWeight.bold)),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
