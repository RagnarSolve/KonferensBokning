import { useState, useEffect } from "react";
import { useBookings } from "../hooks/useBookings";
import { useCustomers } from "../hooks/useCustomers";
import { useFacilities } from "../hooks/useFacilities";
import BookingDetailsModal from "../components/BookingDetailsModal";
import RescheduleModal from "../components/RescheduleModal";
import "../styles/bookingsPage.css";

const BookingsPage = () => {
  const {
    bookings,
    loading,
    fetchAllBookings,
    confirmBooking,
    cancelBooking,
    fetchFilteredBookings,
  } = useBookings();
  const { customers, fetchAllCustomers } = useCustomers();
  const { facilities, fetchAllFacilities } = useFacilities();

  const [selectedBooking, setSelectedBooking] = useState(null);
  const [showDetailsModal, setShowDetailsModal] = useState(false);
  const [showRescheduleModal, setShowRescheduleModal] = useState(false);

  const [filters, setFilters] = useState({
    customerId: "",
    facilityId: "",
    from: "",
    to: "",
  });

  useEffect(() => {
    fetchAllBookings();
    fetchAllCustomers();
    fetchAllFacilities();
  }, []);

  const handleFilter = () => {
    const { customerId, facilityId, from, to } = filters;
    fetchFilteredBookings(
      customerId || null,
      facilityId || null,
      from ? from + "T00:00:00Z" : null,
      to ? to + "T23:59:59Z" : null
    );
  };

  const handleClearFilters = () => {
    setFilters({ customerId: "", facilityId: "", from: "", to: "" });
    fetchAllBookings();
  };

  const handleConfirm = async (id) => {
    if (window.confirm("Bekräfta denna bokning?")) {
      try {
        await confirmBooking(id);
        alert("Bokning bekräftad!");
      } catch (error) {
        alert("Kunde inte bekräfta bokning: " + error.message);
      }
    }
  };

  const handleCancel = async (id) => {
    const reason = prompt("Ange anledning till avbokning (valfritt):");
    if (reason !== null) {
      try {
        await cancelBooking(id, reason || null);
        alert("Bokning avbokad!");
      } catch (error) {
        alert("Kunde inte avboka: " + error.message);
      }
    }
  };

  const handleViewDetails = (booking) => {
    setSelectedBooking(booking);
    setShowDetailsModal(true);
  };

  const handleReschedule = (booking) => {
    setSelectedBooking(booking);
    setShowRescheduleModal(true);
  };

  const getStatusBadge = (status) => {
    const badges = {
      Pending: { text: "Väntande", class: "status-pending" },
      Confirmed: { text: "Bekräftad", class: "status-confirmed" },
      Cancelled: { text: "Avbokad", class: "status-cancelled" },
    };
    const badge = badges[status] || { text: status, class: "" };
    return <span className={`status-badge ${badge.class}`}>{badge.text}</span>;
  };

  if (loading && bookings.length === 0) {
    return <div className="loading">Laddar bokningar...</div>;
  }

  return (
    <div className="bookings-page">
      <div className="bookings-header">
        <h1>Bokningar</h1>
      </div>

      <div className="bookings-filters">
        <div className="filter-group">
          <label>Kund:</label>
          <select
            value={filters.customerId}
            onChange={(e) =>
              setFilters({ ...filters, customerId: e.target.value })
            }
          >
            <option value="">Alla kunder</option>
            {customers.map((c) => (
              <option key={c.id} value={c.id}>
                {c.firstName} {c.lastName}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-group">
          <label>Facility:</label>
          <select
            value={filters.facilityId}
            onChange={(e) =>
              setFilters({ ...filters, facilityId: e.target.value })
            }
          >
            <option value="">Alla facilities</option>
            {facilities.map((f) => (
              <option key={f.id} value={f.id}>
                {f.name}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-group">
          <label>Från:</label>
          <input
            type="date"
            value={filters.from}
            onChange={(e) => setFilters({ ...filters, from: e.target.value })}
          />
        </div>

        <div className="filter-group">
          <label>Till:</label>
          <input
            type="date"
            value={filters.to}
            onChange={(e) => setFilters({ ...filters, to: e.target.value })}
          />
        </div>

        <button className="filter-btn" onClick={handleFilter}>
          Filtrera
        </button>
        <button className="clear-btn" onClick={handleClearFilters}>
          Rensa
        </button>
      </div>

      <div className="bookings-table-container">
        <table className="bookings-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Kund</th>
              <th>Facility</th>
              <th>Startdatum</th>
              <th>Slutdatum</th>
              <th>Deltagare</th>
              <th>Pris</th>
              <th>Status</th>
              <th>Åtgärder</th>
            </tr>
          </thead>
          <tbody>
            {bookings.length === 0 ? (
              <tr>
                <td colSpan="9" className="no-data">
                  Inga bokningar hittades
                </td>
              </tr>
            ) : (
              bookings
                .sort((a, b) => b.id - a.id)
                .map((booking) => (
                  <tr key={booking.id}>
                    <td>{booking.id}</td>
                    <td>{booking.customerName || "-"}</td>
                    <td>{booking.facilityName || "-"}</td>
                    <td>{new Date(booking.startDate).toLocaleDateString()}</td>
                    <td>{new Date(booking.endDate).toLocaleDateString()}</td>
                    <td>{booking.numberOfParticipants || "-"}</td>
                    <td>{booking.totalPrice} kr</td>
                    <td>{getStatusBadge(booking.status)}</td>
                    <td className="actions">
                      <button
                        className="view-btn"
                        onClick={() => handleViewDetails(booking)}
                      >
                        Visa
                      </button>
                      {booking.status === "Pending" && (
                        <button
                          className="confirm-btn"
                          onClick={() => handleConfirm(booking.id)}
                        >
                          Bekräfta
                        </button>
                      )}
                      {booking.status !== "Cancelled" && (
                        <>
                          <button
                            className="reschedule-btn"
                            onClick={() => handleReschedule(booking)}
                          >
                            Omboka
                          </button>
                          <button
                            className="cancel-btn"
                            onClick={() => handleCancel(booking.id)}
                          >
                            Avboka
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))
            )}
          </tbody>
        </table>
      </div>

      {showDetailsModal && selectedBooking && (
        <BookingDetailsModal
          booking={selectedBooking}
          onClose={() => setShowDetailsModal(false)}
        />
      )}

      {showRescheduleModal && selectedBooking && (
        <RescheduleModal
          booking={selectedBooking}
          onClose={() => {
            setShowRescheduleModal(false);
            fetchAllBookings();
          }}
        />
      )}
    </div>
  );
};

export default BookingsPage;
