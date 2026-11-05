import { useState } from "react";
import Productos from "./componentes/Productos.jsx";
import Clientes from "./componentes/Clientes.jsx";
import Pedidos from "./componentes/Pedidos.jsx";
import Autenticacion from "./componentes/Autenticacion.jsx";
import { guardarToken } from "./api.js";

function App() {
  const [pestana, setPestana] = useState("pedidos");
  const [usuario, setUsuario] = useState(() => {
    const token = localStorage.getItem("cafeteria_token");
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")));
      if (payload.exp * 1000 <= Date.now()) { guardarToken(null); return null; }
      return { username: payload.unique_name || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "Usuario", rol: payload.role || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || "EmpleadoAuxiliar" };
    } catch { guardarToken(null); return null; }
  });

  function cerrarSesion() {
    guardarToken(null);
    setUsuario(null);
  }

  if (!usuario) return <Autenticacion onAutenticado={setUsuario} />;

  return (
    <div className="pagina">
      <header className="encabezado">
        <h1>Cafetería</h1>
        <nav>
          <button className={pestana === "pedidos" ? "tab activa" : "tab"} onClick={() => setPestana("pedidos")}>Pedidos</button>
          <button className={pestana === "productos" ? "tab activa" : "tab"} onClick={() => setPestana("productos")}>Productos</button>
          <button className={pestana === "clientes" ? "tab activa" : "tab"} onClick={() => setPestana("clientes")}>Clientes</button>
        </nav>
        <div className="auth-usuario">
          <span>{usuario.nombre || usuario.username} ({usuario.rol || "EmpleadoAuxiliar"})</span>
          <button className="secundario" onClick={cerrarSesion}>Cerrar sesión</button>
        </div>
      </header>
      <main className="contenido">
        {pestana === "pedidos" && <Pedidos />}
        {pestana === "productos" && <Productos />}
        {pestana === "clientes" && <Clientes />}
      </main>
    </div>
  );
}

export default App;
