--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.2
-- Dumped by pg_dump version 9.6.2

-- Started on 2017-03-08 10:22:05

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 8 (class 2615 OID 16410)
-- Name: mongoosedb; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA mongoosedb;


ALTER SCHEMA mongoosedb OWNER TO postgres;

--
-- TOC entry 1 (class 3079 OID 12387)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 3540 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- TOC entry 2 (class 3079 OID 24600)
-- Name: postgis; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS postgis WITH SCHEMA public;


--
-- TOC entry 3541 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION postgis; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION postgis IS 'PostGIS geometry, geography, and raster spatial types and functions';


SET search_path = mongoosedb, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 188 (class 1259 OID 16447)
-- Name: Group; Type: TABLE; Schema: mongoosedb; Owner: postgres
--

CREATE TABLE "Group" (
    "GroupId" integer NOT NULL,
    "Group" text NOT NULL
);


ALTER TABLE "Group" OWNER TO postgres;

--
-- TOC entry 187 (class 1259 OID 16445)
-- Name: Group_GroupId_seq; Type: SEQUENCE; Schema: mongoosedb; Owner: postgres
--

CREATE SEQUENCE "Group_GroupId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "Group_GroupId_seq" OWNER TO postgres;

--
-- TOC entry 3542 (class 0 OID 0)
-- Dependencies: 187
-- Name: Group_GroupId_seq; Type: SEQUENCE OWNED BY; Schema: mongoosedb; Owner: postgres
--

ALTER SEQUENCE "Group_GroupId_seq" OWNED BY "Group"."GroupId";


--
-- TOC entry 190 (class 1259 OID 16460)
-- Name: Individual; Type: TABLE; Schema: mongoosedb; Owner: postgres
--

CREATE TABLE "Individual" (
    "IndividualId" integer NOT NULL,
    "Id" text,
    "Sex" text,
    "GroupId" integer NOT NULL
);


ALTER TABLE "Individual" OWNER TO postgres;

--
-- TOC entry 189 (class 1259 OID 16458)
-- Name: Individual_IndividualId_seq; Type: SEQUENCE; Schema: mongoosedb; Owner: postgres
--

CREATE SEQUENCE "Individual_IndividualId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "Individual_IndividualId_seq" OWNER TO postgres;

--
-- TOC entry 3543 (class 0 OID 0)
-- Dependencies: 189
-- Name: Individual_IndividualId_seq; Type: SEQUENCE OWNED BY; Schema: mongoosedb; Owner: postgres
--

ALTER SEQUENCE "Individual_IndividualId_seq" OWNED BY "Individual"."IndividualId";


--
-- TOC entry 207 (class 1259 OID 26083)
-- Name: weight; Type: TABLE; Schema: mongoosedb; Owner: postgres
--

CREATE TABLE Weight (
    "WeightId" integer NOT NULL,
    "Weight" integer NOT NULL,
    "Accuracy" integer,
    "Collar" integer,
    "Date" date NOT NULL,
    "Time" time without time zone,
    "DateEntered" timestamp without time zone,
    "Location" public.geography,
    "Session" text,
    "IndividualId" integer NOT NULL
);


ALTER TABLE weight OWNER TO postgres;

--
-- TOC entry 206 (class 1259 OID 26081)
-- Name: weight_WeightId_seq; Type: SEQUENCE; Schema: mongoosedb; Owner: postgres
--

CREATE SEQUENCE "weight_WeightId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE "weight_WeightId_seq" OWNER TO postgres;

--
-- TOC entry 3544 (class 0 OID 0)
-- Dependencies: 206
-- Name: weight_WeightId_seq; Type: SEQUENCE OWNED BY; Schema: mongoosedb; Owner: postgres
--

ALTER SEQUENCE "weight_WeightId_seq" OWNED BY weight."WeightId";


--
-- TOC entry 3396 (class 2604 OID 16450)
-- Name: Group GroupId; Type: DEFAULT; Schema: mongoosedb; Owner: postgres
--

ALTER TABLE ONLY "Group" ALTER COLUMN "GroupId" SET DEFAULT nextval('"Group_GroupId_seq"'::regclass);


--
-- TOC entry 3397 (class 2604 OID 16463)
-- Name: Individual IndividualId; Type: DEFAULT; Schema: mongoosedb; Owner: postgres
--

ALTER TABLE ONLY "Individual" ALTER COLUMN "IndividualId" SET DEFAULT nextval('"Individual_IndividualId_seq"'::regclass);


--
-- TOC entry 3399 (class 2604 OID 26086)
-- Name: weight WeightId; Type: DEFAULT; Schema: mongoosedb; Owner: postgres
--

ALTER TABLE ONLY weight ALTER COLUMN "WeightId" SET DEFAULT nextval('"weight_WeightId_seq"'::regclass);


--
-- TOC entry 3401 (class 2606 OID 16457)
-- Name: Group GroupUnique; Type: CONSTRAINT; Schema: mongoosedb; Owner: postgres
--

ALTER TABLE ONLY "Group"
    ADD CONSTRAINT "GroupUnique" UNIQUE ("Group");


--
-- TOC entry 3403 (class 2606 OID 16455)
-- Name: Group Group_pkey; Type: CONSTRAINT; Schema: mongoosedb; Owner: postgres
--

ALTER TABLE ONLY "Group"
    ADD CONSTRAINT "Group_pkey" PRIMARY KEY ("GroupId");


--
-- TOC entry 3405 (class 2606 OID 16465)
-- Name: Individual Individual_pkey; Type: CONSTRAINT; Schema: mongoosedb; Owner: postgres
--

ALTER TABLE ONLY "Individual"
    ADD CONSTRAINT "Individual_pkey" PRIMARY KEY ("IndividualId");


--
-- TOC entry 3407 (class 2606 OID 26091)
-- Name: weight weight_pkey; Type: CONSTRAINT; Schema: mongoosedb; Owner: postgres
--

ALTER TABLE ONLY weight
    ADD CONSTRAINT weight_pkey PRIMARY KEY ("WeightId");


--
-- TOC entry 3408 (class 2606 OID 16469)
-- Name: Individual Individual_GroupId_fkey; Type: FK CONSTRAINT; Schema: mongoosedb; Owner: postgres
--

ALTER TABLE ONLY "Individual"
    ADD CONSTRAINT "Individual_GroupId_fkey" FOREIGN KEY ("GroupId") REFERENCES "Group"("GroupId");


--
-- TOC entry 3409 (class 2606 OID 26092)
-- Name: weight fk_individual; Type: FK CONSTRAINT; Schema: mongoosedb; Owner: postgres
--

ALTER TABLE ONLY weight
    ADD CONSTRAINT fk_individual FOREIGN KEY ("IndividualId") REFERENCES "Individual"("IndividualId");


-- Completed on 2017-03-08 10:22:06

--
-- PostgreSQL database dump complete
--

