////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.dtos.securite;

import java.io.Serializable;
import java.util.List;
import java.util.stream.Collectors;

import jakarta.annotation.Generated;

import topmodel.jpa.sample.demo.dtos.utilisateur.UtilisateurDto;
import topmodel.jpa.sample.demo.entities.securite.Droits;
import topmodel.jpa.sample.demo.entities.securite.Profil;
import topmodel.jpa.sample.demo.entities.securite.TypeProfil;

/**
 * Objet métier non persisté représentant Profil.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public class ProfilDto implements Serializable {
	/** Serial ID */
	private static final long serialVersionUID = 1L;

	/**
	 * Id technique.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.Profil#getId() Profil#getId()} 
	 */
	private Long id;

	/**
	 * Type de profil.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.Profil#getTypeProfils() Profil#getTypeProfils()} 
	 */
	private List<TypeProfil.Values> typeProfils;

	/**
	 * Liste des droits de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.Profil#getDroits() Profil#getDroits()} 
	 */
	private List<Droits.Values> droits;

	/**
	 * Liste des secteurs de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.Profil#getSecteurs() Profil#getSecteurs()} 
	 */
	private List<Long> secteurs;

	/**
	 * Liste paginée des utilisateurs de ce profil.
	 */
	private List<UtilisateurDto> utilisateurs;

	/**
	 * No arg constructor.
	 */
	public ProfilDto() {
	}

	/**
	 * Copy constructor.
	 * @param profilDto to copy
	 */
	public ProfilDto(ProfilDto profilDto) {
		if(profilDto == null) {
			return;
		}

		this.id = profilDto.getId();
		this.typeProfils = profilDto.getTypeProfils();
		this.droits = profilDto.getDroits();
		this.secteurs = profilDto.getSecteurs();

		this.utilisateurs = profilDto.getUtilisateurs().stream().collect(Collectors.toList());

	}

	/**
	 * All arg constructor.
	 * @param id Id technique
	 * @param typeProfils Type de profil
	 * @param droits Liste des droits de l'utilisateur
	 * @param secteurs Liste des secteurs de l'utilisateur
	 * @param utilisateurs Liste paginée des utilisateurs de ce profil
	 */
	public ProfilDto(Long id, List<TypeProfil.Values> typeProfils, List<Droits.Values> droits, List<Long> secteurs, List<UtilisateurDto> utilisateurs) {
		this.id = id;
		this.typeProfils = typeProfils;
		this.droits = droits;
		this.secteurs = secteurs;
		this.utilisateurs = utilisateurs;
	}

	/**
	 * Crée une nouvelle instance de 'ProfilDto'.
	 * @param profil Instance de 'Profil'.
	 *
	 * @return Une nouvelle instance de 'ProfilDto'.
	 */
	public ProfilDto(Profil profil) {
		this.from(profil);
	}

	/**
	 * Map les champs des classes passées en paramètre dans l'instance courante.
	 * @param profil Instance de 'Profil'.
	 */
	protected void from(Profil profil) {
		if (profil != null) {
			this.id = profil.getId();
			this.setTypeProfils(profil.getTypeProfils());
			this.setDroits(profil.getDroits());
			if (profil.getSecteurs() != null) {
				this.secteurs = profil.getSecteurs().stream().filter(t -> t != null).map(secteurs -> secteurs.getId()).collect(Collectors.toList());
			}
		} else {
			throw new IllegalArgumentException("profil cannot be null");
		}
	}

	/**
	 * Getter for id.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.ProfilDto#id id}.
	 */
	public Long getId() {
		return this.id;
	}

	/**
	 * Getter for typeProfils.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.ProfilDto#typeProfils typeProfils}.
	 */
	public List<TypeProfil.Values> getTypeProfils() {
		return this.typeProfils;
	}

	/**
	 * Getter for droits.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.ProfilDto#droits droits}.
	 */
	public List<Droits.Values> getDroits() {
		return this.droits;
	}

	/**
	 * Getter for secteurs.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.ProfilDto#secteurs secteurs}.
	 */
	public List<Long> getSecteurs() {
		return this.secteurs;
	}

	/**
	 * Getter for utilisateurs.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.ProfilDto#utilisateurs utilisateurs}.
	 */
	public List<UtilisateurDto> getUtilisateurs() {
		return this.utilisateurs;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.ProfilDto#id id}.
	 * @param id value to set
	 */
	public void setId(Long id) {
		this.id = id;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.ProfilDto#typeProfils typeProfils}.
	 * @param typeProfils value to set
	 */
	public void setTypeProfils(List<TypeProfil.Values> typeProfils) {
		this.typeProfils = typeProfils;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.ProfilDto#droits droits}.
	 * @param droits value to set
	 */
	public void setDroits(List<Droits.Values> droits) {
		this.droits = droits;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.ProfilDto#secteurs secteurs}.
	 * @param secteurs value to set
	 */
	public void setSecteurs(List<Long> secteurs) {
		this.secteurs = secteurs;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.ProfilDto#utilisateurs utilisateurs}.
	 * @param utilisateurs value to set
	 */
	public void setUtilisateurs(List<UtilisateurDto> utilisateurs) {
		this.utilisateurs = utilisateurs;
	}

	/**
	 * Mappe 'ProfilDto' vers 'Profil'.
	 * @param source Instance de 'ProfilDto'.
	 * @param dest Instance pré-existante de 'Profil'. Une nouvelle instance sera créée si non spécifié.
	 *
	 * @return Une instance de 'Profil'.
	 */
	public Profil toProfil(Profil dest) {
		dest = dest == null ? new Profil() : dest;

		dest.setId(this.getId());
		dest.setTypeProfils(this.getTypeProfils());
		dest.setDroits(this.getDroits());

		return dest;
	}
}
