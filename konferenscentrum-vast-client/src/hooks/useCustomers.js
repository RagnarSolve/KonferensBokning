import { useState } from "react";
import { customerApi } from "../api/customerApi";

export const useCustomers = () => {
  const [customers, setCustomers] = useState([]);
  const [customer, setCustomer] = useState(null);
  const [loading, setLoading] = useState(false);

  const fetchAllCustomers = async () => {
    setLoading(true);
    try {
      const data = await customerApi.getAll();
      setCustomers(data);
    } finally {
      setLoading(false);
    }
  };

  const fetchCustomerById = async (id) => {
    setLoading(true);
    try {
      const data = await customerApi.getById(id);
      setCustomer(data);
      return data;
    } finally {
      setLoading(false);
    }
  };

  const fetchCustomerByEmail = async (email) => {
    setLoading(true);
    try {
      const data = await customerApi.getByEmail(email);
      setCustomer(data);
      return data;
    } finally {
      setLoading(false);
    }
  };

  const createCustomer = async (customerData) => {
    setLoading(true);
    try {
      const newCustomer = await customerApi.create(customerData);
      setCustomers((prev) => [...prev, newCustomer]);
      return newCustomer;
    } finally {
      setLoading(false);
    }
  };

  const updateCustomer = async (id, customerData) => {
    setLoading(true);
    try {
      const updatedCustomer = await customerApi.update(id, customerData);
      setCustomers((prev) =>
        prev.map((c) => (c.id === id ? updatedCustomer : c))
      );
      setCustomer(updatedCustomer);
      return updatedCustomer;
    } finally {
      setLoading(false);
    }
  };

  const deleteCustomer = async (id) => {
    setLoading(true);
    try {
      await customerApi.delete(id);
      setCustomers((prev) => prev.filter((c) => c.id !== id));
      if (customer?.id === id) {
        setCustomer(null);
      }
    } finally {
      setLoading(false);
    }
  };

  return {
    customers,
    customer,
    loading,
    fetchAllCustomers,
    fetchCustomerById,
    fetchCustomerByEmail,
    createCustomer,
    updateCustomer,
    deleteCustomer,
  };
};
