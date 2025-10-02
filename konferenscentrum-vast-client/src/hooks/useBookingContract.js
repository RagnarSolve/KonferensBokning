import { useState } from "react";
import { bookingContractApi } from "../api/bookingContractApi";

export const useBookingContracts = () => {
  const [contracts, setContracts] = useState([]);
  const [contract, setContract] = useState(null);
  const [loading, setLoading] = useState(false);

  const fetchAllContracts = async () => {
    setLoading(true);
    try {
      const data = await bookingContractApi.getAll();
      setContracts(data);
    } finally {
      setLoading(false);
    }
  };

  const fetchContractById = async (id) => {
    setLoading(true);
    try {
      const data = await bookingContractApi.getById(id);
      setContract(data);
      return data;
    } finally {
      setLoading(false);
    }
  };

  const fetchContractByBookingId = async (bookingId) => {
    setLoading(true);
    try {
      const data = await bookingContractApi.getByBookingId(bookingId);
      setContract(data);
      return data;
    } finally {
      setLoading(false);
    }
  };

  const createContract = async (bookingId, contractData) => {
    setLoading(true);
    try {
      const newContract = await bookingContractApi.createContract(
        bookingId,
        contractData
      );
      setContracts((prev) => [...prev, newContract]);
      return newContract;
    } finally {
      setLoading(false);
    }
  };

  const updateContract = async (id, contractData) => {
    setLoading(true);
    try {
      const updatedContract = await bookingContractApi.patch(id, contractData);
      setContracts((prev) =>
        prev.map((c) => (c.id === id ? updatedContract : c))
      );
      setContract(updatedContract);
      return updatedContract;
    } finally {
      setLoading(false);
    }
  };

  const markContractSent = async (id) => {
    setLoading(true);
    try {
      const updatedContract = await bookingContractApi.markSent(id);
      setContracts((prev) =>
        prev.map((c) => (c.id === id ? updatedContract : c))
      );
      setContract(updatedContract);
      return updatedContract;
    } finally {
      setLoading(false);
    }
  };

  const markContractSigned = async (id) => {
    setLoading(true);
    try {
      const updatedContract = await bookingContractApi.markSigned(id);
      setContracts((prev) =>
        prev.map((c) => (c.id === id ? updatedContract : c))
      );
      setContract(updatedContract);
      return updatedContract;
    } finally {
      setLoading(false);
    }
  };

  const cancelContract = async (id, reason) => {
    setLoading(true);
    try {
      const updatedContract = await bookingContractApi.cancel(id, reason);
      setContracts((prev) =>
        prev.map((c) => (c.id === id ? updatedContract : c))
      );
      setContract(updatedContract);
      return updatedContract;
    } finally {
      setLoading(false);
    }
  };

  return {
    contracts,
    contract,
    loading,
    fetchAllContracts,
    fetchContractById,
    fetchContractByBookingId,
    createContract,
    updateContract,
    markContractSent,
    markContractSigned,
    cancelContract,
  };
};
