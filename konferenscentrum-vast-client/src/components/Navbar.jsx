import { Link } from "react-router-dom";
import "../styles/navbar.css";

const Navbar = () => {
  return (
    <nav className="navbar">
      <div className="navbar-container">
        <Link to="/" className="navbar-logo">
          Konferensrum Väst
        </Link>

        <div className="navbar-links">
          <Link to="/" className="navbar-link">
            Hem
          </Link>
          <Link to="/customers" className="navbar-link">
            Kunder
          </Link>
          <Link to="/bookings" className="navbar-link">
            Bokningar
          </Link>
          <Link to="/facilities" className="navbar-link">
            Anläggningar
          </Link>
          <Link to="/booking-contracts" className="navbar-link">
            Bokningskontrakt
          </Link>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
