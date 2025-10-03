import { useState, useEffect } from "react";
import { useFacilities } from "../hooks/useFacilities";
import FacilityFormModal from "../components/FacilityFormModal";
import "../styles/facilitiesPage.css";

const FacilitiesPage = () => {
  const {
    facilities,
    loading,
    fetchAllFacilities,
    deleteFacility,
    setActiveFacility,
  } = useFacilities();

  const [showModal, setShowModal] = useState(false);
  const [editFacility, setEditFacility] = useState(null);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    fetchAllFacilities();
  }, []);

  const handleEdit = (facility) => {
    setEditFacility(facility);
    setShowModal(true);
  };

  const handleDelete = async (id) => {
    if (window.confirm("Är du säker på att du vill ta bort denna facility?")) {
      try {
        await deleteFacility(id);
        alert("Facility borttagen!");
      } catch (error) {
        alert("Kunde inte ta bort facility: " + error.message);
      }
    }
  };

  const handleToggleActive = async (id, currentStatus) => {
    try {
      await setActiveFacility(id, !currentStatus);
    } catch (error) {
      alert("Kunde inte ändra status: " + error.message);
    }
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditFacility(null);
    fetchAllFacilities();
  };

  const filteredFacilities = facilities.filter((facility) => {
    const searchLower = searchTerm.toLowerCase();
    const name = (facility.name || "").toLowerCase();
    const city = (facility.city || "").toLowerCase();
    return name.includes(searchLower) || city.includes(searchLower);
  });

  if (loading && facilities.length === 0) {
    return <div className="loading">Laddar facilities...</div>;
  }

  return (
    <div className="facilities-admin-page">
      <div className="facilities-admin-header">
        <h1>Hantera Facilities</h1>
        <button className="add-btn" onClick={() => setShowModal(true)}>
          + Lägg till facility
        </button>
      </div>

      <div className="facilities-search">
        <input
          type="text"
          placeholder="Sök efter namn eller stad..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>

      <div className="facilities-table-container">
        <table className="facilities-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Namn</th>
              <th>Stad</th>
              <th>Kapacitet</th>
              <th>Pris/dag</th>
              <th>Status</th>
              <th>Åtgärder</th>
            </tr>
          </thead>
          <tbody>
            {filteredFacilities.length === 0 ? (
              <tr>
                <td colSpan="7" className="no-data">
                  {searchTerm
                    ? "Inga facilities hittades"
                    : "Inga facilities ännu"}
                </td>
              </tr>
            ) : (
              filteredFacilities
                .sort((a, b) => a.id - b.id)
                .map((facility) => (
                  <tr
                    key={facility.id}
                    className={!facility.isActive ? "inactive-row" : ""}
                  >
                    <td>{facility.id}</td>
                    <td>{facility.name}</td>
                    <td>{facility.city}</td>
                    <td>{facility.maxCapacity}</td>
                    <td>{facility.pricePerDay} kr</td>
                    <td>
                      <label className="toggle-switch">
                        <input
                          type="checkbox"
                          checked={facility.isActive}
                          onChange={() =>
                            handleToggleActive(facility.id, facility.isActive)
                          }
                        />
                        <span className="toggle-slider"></span>
                      </label>
                      <span
                        className={
                          facility.isActive
                            ? "status-active"
                            : "status-inactive"
                        }
                      >
                        {facility.isActive ? "Aktiv" : "Inaktiv"}
                      </span>
                    </td>
                    <td className="actions">
                      <button
                        className="edit-btn"
                        onClick={() => handleEdit(facility)}
                      >
                        Redigera
                      </button>
                      <button
                        className="delete-btn"
                        onClick={() => handleDelete(facility.id)}
                      >
                        Ta bort
                      </button>
                    </td>
                  </tr>
                ))
            )}
          </tbody>
        </table>
      </div>

      {showModal && (
        <FacilityFormModal facility={editFacility} onClose={handleCloseModal} />
      )}
    </div>
  );
};

export default FacilitiesPage;
