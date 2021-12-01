////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.exemple.name.api.controller.utilisateur;

import java.util.List;

import javax.validation.constraints.Email;
import javax.validation.Valid;

import org.springframework.data.domain.Page;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import topmodel.exemple.name.dao.dtos.utilisateur.UtilisateurDto;
import topmodel.exemple.name.dao.references.utilisateur.TypeUtilisateurCode;

@RestController
@RequestMapping("utilisateur")
public interface IutilisateurController {


    /**
     * Charge le détail d'un utilisateur.
     * @param utilisateurId Id technique
     * @return Le détail de l'utilisateur
     */
    @GetMapping("/{utiId}")
    default UtilisateurDto getUtilisateurMapping(@RequestParam("utilisateurId") long utilisateurId) {
        return this.getUtilisateur(utilisateurId);
    }

    /**
     * Charge le détail d'un utilisateur.
     * @param utilisateurId Id technique
     * @return Le détail de l'utilisateur
     */
    UtilisateurDto getUtilisateur(long utilisateurId);

    /**
     * Charge une liste d'utilisateurs par leur type.
     * @param typeUtilisateurCode Type d'utilisateur en Many to one
     * @return Liste des utilisateurs
     */
    @GetMapping("/list")
    default List<UtilisateurDto> getUtilisateurListMapping(@RequestParam("typeUtilisateurCode") TypeUtilisateurCode typeUtilisateurCode) {
        return this.getUtilisateurList(typeUtilisateurCode);
    }

    /**
     * Charge une liste d'utilisateurs par leur type.
     * @param typeUtilisateurCode Type d'utilisateur en Many to one
     * @return Liste des utilisateurs
     */
    List<UtilisateurDto> getUtilisateurList(TypeUtilisateurCode typeUtilisateurCode);

    /**
     * Sauvegarde un utilisateur.
     * @param utilisateur Utilisateur à sauvegarder
     * @return Utilisateur sauvegardé
     */
    @PostMapping("/save")
    default UtilisateurDto saveUtilisateurMapping(@RequestBody @Valid UtilisateurDto utilisateur) {
        return this.saveUtilisateur(utilisateur);
    }

    /**
     * Sauvegarde un utilisateur.
     * @param utilisateur Utilisateur à sauvegarder
     * @return Utilisateur sauvegardé
     */
    UtilisateurDto saveUtilisateur(UtilisateurDto utilisateur);

    /**
     * Sauvegarde une liste d'utilisateurs.
     * @param utilisateur Utilisateur à sauvegarder
     * @return Utilisateur sauvegardé
     */
    @PostMapping("/saveAll")
    default List<UtilisateurDto> saveAllUtilisateurMapping(@RequestBody @Valid List<UtilisateurDto> utilisateur) {
        return this.saveAllUtilisateur(utilisateur);
    }

    /**
     * Sauvegarde une liste d'utilisateurs.
     * @param utilisateur Utilisateur à sauvegarder
     * @return Utilisateur sauvegardé
     */
    List<UtilisateurDto> saveAllUtilisateur(List<UtilisateurDto> utilisateur);

    /**
     * Recherche des utilisateurs.
     * @param utilisateurId Id technique
     * @param email Email de l'utilisateur
     * @param typeUtilisateurCode Type d'utilisateur en Many to one
     * @param typeUtilisateurCodeOneToOneType Type d'utilisateur en one to one
     * @return Utilisateurs matchant les critères
     */
    @PostMapping("/search")
    default Page<UtilisateurDto> searchMapping(@RequestParam("utilisateurId") long utilisateurId, @RequestParam("email") String email, @RequestParam("typeUtilisateurCode") TypeUtilisateurCode typeUtilisateurCode, @RequestParam("typeUtilisateurCodeOneToOneType") TypeUtilisateurCode typeUtilisateurCodeOneToOneType) {
        return this.search(utilisateurId, email, typeUtilisateurCode, typeUtilisateurCodeOneToOneType);
    }

    /**
     * Recherche des utilisateurs.
     * @param utilisateurId Id technique
     * @param email Email de l'utilisateur
     * @param typeUtilisateurCode Type d'utilisateur en Many to one
     * @param typeUtilisateurCodeOneToOneType Type d'utilisateur en one to one
     * @return Utilisateurs matchant les critères
     */
    Page<UtilisateurDto> search(long utilisateurId, String email, TypeUtilisateurCode typeUtilisateurCode, TypeUtilisateurCode typeUtilisateurCodeOneToOneType);
}