import 'package:flutter/material.dart';
import '../../services/auth_service.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});
  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _pass = TextEditingController();
  bool _isLoading = false;
  bool _obscure = true;

  void _register() async {
    if (_name.text.isEmpty || _email.text.isEmpty || _pass.text.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Proszę wypełnić wszystkie pola")));
      return;
    }

    setState(() => _isLoading = true);
    bool ok = await AuthService().register(_name.text, _email.text, _pass.text);
    if (mounted) {
      setState(() => _isLoading = false);
      if (ok) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Konto utworzone! Możesz się teraz zalogować."), backgroundColor: Colors.green));
        Navigator.pop(context);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Rejestracja nieudana. Email może być już zajęty."), backgroundColor: Colors.redAccent));
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(backgroundColor: Colors.white, foregroundColor: Colors.black, elevation: 0),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 30),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text("Utwórz Konto", style: TextStyle(fontSize: 32, fontWeight: FontWeight.bold, color: Colors.deepOrange)),
              const Text("Dołącz do naszej sieci restauracji (RES-01)", style: TextStyle(color: Colors.grey)),
              const SizedBox(height: 40),
              TextField(
                controller: _name,
                decoration: const InputDecoration(labelText: 'Imię i Nazwisko', prefixIcon: Icon(Icons.person_outline)),
              ),
              const SizedBox(height: 20),
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
              const SizedBox(height: 40),
              _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : ElevatedButton(onPressed: _register, child: const Text("ZAREJESTRUJ SIĘ")),
              const SizedBox(height: 20),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Text("Masz już konto? "),
                  GestureDetector(
                    onTap: () => Navigator.pop(context),
                    child: const Text("Zaloguj się", style: TextStyle(color: Colors.deepOrange, fontWeight: FontWeight.bold)),
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
