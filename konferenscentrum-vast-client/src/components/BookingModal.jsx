import { useState, useEffect } from "react";
import { useBookings } from "../hooks/useBookings";
import "../styles/bookingModal.css";

const BookingModal = ({ facility, onClose }) => {
  const { bookings, loading, fetchFilteredBookings, createBooking } =
    useBookings();
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [customerId, setCustomerId] = useState("");
  const [numberOfParticipants, setNumberOfParticipants] = useState("");
  const [notes, setNotes] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    fetchFilteredBookings(null, facility.id, null, null);
  }, [facility.id]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!startDate || !endDate || !customerId || !numberOfParticipants) {
      setError("Alla obligatoriska fält måste fyllas i");
      return;
    }

    if (parseInt(numberOfParticipants) > facility.maxCapacity) {
      setError(`Max antal deltagare är ${facility.maxCapacity}`);
      return;
    }

    try {
      await createBooking({
        customerId: parseInt(customerId),
        facilityId: facility.id,
        startDate: startDate + "T00:00:00Z",
        endDate: endDate + "T23:59:59Z",
        numberOfParticipants: parseInt(numberOfParticipants),
        notes: notes || null,
      });
      setSuccess(true);
      setTimeout(() => {
        onClose();
      }, 2000);
    } catch (err) {
      setError(err.message || "Något gick fel vid bokningen");
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>
          ×
        </button>

        <h2>Boka {facility.name}</h2>

        {success ? (
          <div className="success-message">✓ Bokning genomförd!</div>
        ) : (
          <>
            <div className="booked-dates">
              <h3>Bokade datum:</h3>
              {loading ? (
                <p>Laddar...</p>
              ) : bookings.length === 0 ? (
                <p>Inga bokningar ännu</p>
              ) : (
                <ul>
                  {bookings.map((booking) => (
                    <li key={booking.id}>
                      {new Date(booking.startDate).toLocaleDateString()} -{" "}
                      {new Date(booking.endDate).toLocaleDateString()}
                      {booking.numberOfParticipants &&
                        ` (${booking.numberOfParticipants} deltagare)`}
                    </li>
                  ))}
                </ul>
              )}
            </div>

            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Kund-ID: *</label>
                <input
                  type="number"
                  value={customerId}
                  onChange={(e) => setCustomerId(e.target.value)}
                  placeholder="Ange ditt kund-ID"
                  min="1"
                />
              </div>

              <div className="form-group">
                <label>Startdatum: *</label>
                <input
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  min={new Date().toISOString().split("T")[0]}
                />
              </div>

              <div className="form-group">
                <label>Slutdatum: *</label>
                <input
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  min={startDate || new Date().toISOString().split("T")[0]}
                />
              </div>

              <div className="form-group">
                <label>Antal deltagare: * (Max {facility.maxCapacity})</label>
                <input
                  type="number"
                  value={numberOfParticipants}
                  onChange={(e) => setNumberOfParticipants(e.target.value)}
                  placeholder="Antal deltagare"
                  min="1"
                  max={facility.maxCapacity}
                />
              </div>

              <div className="form-group">
                <label>Anteckningar (valfritt):</label>
                <textarea
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Eventuella önskemål eller information"
                  rows="3"
                />
              </div>

              {error && <div className="error-message">{error}</div>}

              <button type="submit" className="submit-btn" disabled={loading}>
                {loading ? "Bokar..." : "Bekräfta bokning"}
              </button>
            </form>
          </>
        )}
      </div>
    </div>
  );
};

export default BookingModal;
