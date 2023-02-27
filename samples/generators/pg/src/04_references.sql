﻿----
---- ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
----

-- =========================================================================================== 
--   Application Name	:	pg 
--   Script Name		:	04_references.sql
--   Description		:	Script d'insertion des données de références
-- ===========================================================================================
/**		Initialisation de la table TYPE_PROFIL		**/
INSERT INTO TYPE_PROFIL(TPR_CODE, TPR_LIBELLE) VALUES('ADM', 'Administrateur');
INSERT INTO TYPE_PROFIL(TPR_CODE, TPR_LIBELLE) VALUES('GES', 'Gestionnaire');

/**		Initialisation de la table DROIT		**/
INSERT INTO DROIT(DRO_CODE, DRO_LIBELLE, TPR_CODE) VALUES('CRE', 'Créer', 'ADM');
INSERT INTO DROIT(DRO_CODE, DRO_LIBELLE, TPR_CODE) VALUES('MOD', 'Modifier', null);
INSERT INTO DROIT(DRO_CODE, DRO_LIBELLE, TPR_CODE) VALUES('SUP', 'Supprimer', null);

/**		Initialisation de la table TYPE_UTILISATEUR		**/
INSERT INTO TYPE_UTILISATEUR(TUT_CODE, TUT_LIBELLE) VALUES('ADM', 'Administrateur');
INSERT INTO TYPE_UTILISATEUR(TUT_CODE, TUT_LIBELLE) VALUES('GES', 'Gestionnaire');
INSERT INTO TYPE_UTILISATEUR(TUT_CODE, TUT_LIBELLE) VALUES('CLI', 'Client');

