import "../styles/bookingModal.css";

const BookingDetailsModal = ({ booking, onClose }) => {
  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>
          ×
        </button>

        <h2>Bokningsdetaljer #{booking.id}</h2>

        <div className="details-grid-modal">
          <div className="detail-section">
            <h3>Kund</h3>
            <p>
              <strong>Namn:</strong> {booking.customerName}
            </p>
            <p>
              <strong>E-post:</strong> {booking.customerEmail}
            </p>
          </div>

          <div className="detail-section">
            <h3>Facility</h3>
            <p>
              <strong>Namn:</strong> {booking.facilityName}
            </p>
          </div>

          <div className="detail-section">
            <h3>Bokningsinformation</h3>
            <p>
              <strong>Startdatum:</strong>{" "}
              {new Date(booking.startDate).toLocaleString()}
            </p>
            <p>
              <strong>Slutdatum:</strong>{" "}
              {new Date(booking.endDate).toLocaleString()}
            </p>
            <p>
              <strong>Antal deltagare:</strong> {booking.numberOfParticipants}
            </p>
            <p>
              <strong>Totalpris:</strong> {booking.totalPrice} kr
            </p>
          </div>

          <div className="detail-section">
            <h3>Status</h3>
            <p>
              <strong>Status:</strong> {booking.status}
            </p>
            <p>
              <strong>Skapad:</strong>{" "}
              {new Date(booking.createdDate).toLocaleString()}
            </p>
            {booking.confirmedDate && (
              <p>
                <strong>Bekräftad:</strong>{" "}
                {new Date(booking.confirmedDate).toLocaleString()}
              </p>
            )}
            {booking.cancelledDate && (
              <p>
                <strong>Avbokad:</strong>{" "}
                {new Date(booking.cancelledDate).toLocaleString()}
              </p>
            )}
          </div>

          {booking.notes && (
            <div className="detail-section full-width">
              <h3>Anteckningar</h3>
              <p>{booking.notes}</p>
            </div>
          )}

          {booking.contractId && (
            <div className="detail-section">
              <h3>Kontrakt</h3>
              <p>
                <strong>Kontrakt-ID:</strong> {booking.contractId}
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default BookingDetailsModal;
