import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "./pages/Home";
import BookingContracts from "./pages/BookingContracts";
import BookingsPage from "./pages/BookingsPage";
import CustomersPage from "./pages/CustomersPage";
import FacilitiesPage from "./pages/FacilitiesPage";
import FacilityDetailsPage from "./pages/FacilityDetailsPage";
import Navbar from "./components/Navbar";

function App() {
  return (
    <Router>
      <Navbar />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/customers" element={<CustomersPage />} />
        <Route path="/bookings" element={<BookingsPage />} />
        <Route path="/facilities" element={<FacilitiesPage />} />
        <Route path="/booking-contracts" element={<BookingContracts />} />
        <Route path="/facility/:id" element={<FacilityDetailsPage />} />
      </Routes>
    </Router>
  );
}

export default App;
