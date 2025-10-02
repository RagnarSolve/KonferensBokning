import { useState } from "react";
import { facilityApi } from "../api/facilityApi";

export const useFacilities = () => {
  const [facilities, setFacilities] = useState([]);
  const [facility, setFacility] = useState(null);
  const [loading, setLoading] = useState(false);

const fetchAllFacilities = async () => {
  setLoading(true);
  try {
    const response = await facilityApi.getAll();
    const dataArray = Array.isArray(response) ? response : response.data || [];
    setFacilities(dataArray); 
  } finally {
    setLoading(false);
  }
};

const fetchActiveFacilities = async () => {
  setLoading(true);
  try {
    const response = await facilityApi.getActive();
    const dataArray = Array.isArray(response) ? response : response.data || [];
    setFacilities(dataArray);
  } finally {
    setLoading(false);
  }
};

  const fetchFacilityById = async (id) => {
    setLoading(true);
    try {
      const data = await facilityApi.getById(id);
      setFacility(data);
      return data;
    } finally {
      setLoading(false);
    }
  };

  const createFacility = async (facilityData) => {
    setLoading(true);
    try {
      const newFacility = await facilityApi.create(facilityData);
      setFacilities((prev) => [...prev, newFacility]);
      return newFacility;
    } finally {
      setLoading(false);
    }
  };

  const updateFacility = async (id, facilityData) => {
    setLoading(true);
    try {
      const updatedFacility = await facilityApi.update(id, facilityData);
      setFacilities((prev) =>
        prev.map((f) => (f.id === id ? updatedFacility : f))
      );
      setFacility(updatedFacility);
      return updatedFacility;
    } finally {
      setLoading(false);
    }
  };

  const deleteFacility = async (id) => {
    setLoading(true);
    try {
      await facilityApi.delete(id);
      setFacilities((prev) => prev.filter((f) => f.id !== id));
      if (facility?.id === id) {
        setFacility(null);
      }
    } finally {
      setLoading(false);
    }
  };

  const setActiveFacility = async (id, isActive) => {
    setLoading(true);
    try {
      const updatedFacility = await facilityApi.setActive(id, isActive);
      setFacilities((prev) =>
        prev.map((f) => (f.id === id ? updatedFacility : f))
      );
      setFacility(updatedFacility);
      return updatedFacility;
    } finally {
      setLoading(false);
    }
  };

  return {
    facilities,
    facility,
    loading,
    fetchAllFacilities,
    fetchActiveFacilities,
    fetchFacilityById,
    createFacility,
    updateFacility,
    deleteFacility,
    setActiveFacility,
  };
};
