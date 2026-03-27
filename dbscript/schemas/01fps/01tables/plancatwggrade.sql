-- Table: fps.plancatwggrade

CREATE TABLE fps.plancatwggrade (
    plancategory citext NOT NULL,
    wggrade citext NOT NULL,
    hours integer DEFAULT 0,
    createdby character varying(10),
    selleragrees character varying(10),
    buyeragrees character varying(10),
    fpsyear integer,
    CONSTRAINT pk__plancatwggrade__384464fa PRIMARY KEY (plancategory, wggrade),
    CONSTRAINT fk_plancatwggrade_plancategory FOREIGN KEY (plancategory) REFERENCES fps.tblkpplanningcategory(planningcategory),
    CONSTRAINT fk_plancatwggrade_wggrade FOREIGN KEY (wggrade) REFERENCES fps.workgroupgrade(wggrade)
);

